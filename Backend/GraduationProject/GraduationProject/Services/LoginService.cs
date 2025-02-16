using Ganss.Xss;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GraduationProject.Services
{
    public interface ILoginRegisterService
    {
        //Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload , HttpContext httpContext);
        Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext);
        Task<IResult> Logout(HttpContext httpContext);

        Task<IResult> Register(RegisterCustomRequest model);
    }
    public class LoginRegisterService : ILoginRegisterService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtOptions _jwtOptions;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRepository _appUserRepo;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            SignInManager<AppUser> signInManager,
            AppDbContext context,
            RoleManager<Role> roleManager,
            IUserRepository appUsersRepository,
            ITokenBlacklistService tokenBlacklistService
            )

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _appUserRepo = appUsersRepository;
            _tokenBlacklistService = tokenBlacklistService;
        }


        public async Task<IResult> Register(RegisterCustomRequest model)
        {
            if(await _userManager.FindByEmailAsync(model.Email) != null)
                return Results.BadRequest(new ErrorResponse 
                { 
                    Message = "Email already exists",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" && model.Role != "student")
                return Results.BadRequest(new ErrorResponse()
                {
                   Message = "Invalid Role!",
                   Code = System.Net.HttpStatusCode.BadRequest,
                });

            if (model.Password != model.ConfirmPassword)
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message =  "Passwords do not match",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

            string? RegionCode = null;
            if (model.PhoneNumber != null && model.PhoneRegion != null)
            {
                (string, string)? phoneDetails = PhoneNumberService
                                        .IsValidPhoneNumber(model.PhoneNumber, model.PhoneRegion);
                if (phoneDetails.HasValue)
                {
                    model.PhoneNumber = phoneDetails.Value.Item1;
                    RegionCode = phoneDetails.Value.Item2;
                }
                else
                {
                    return Results.BadRequest(new ErrorResponse() 
                    { 
                        Message = "Invalid phone number",
                        Code = System.Net.HttpStatusCode.BadRequest 
                    });
                }
            }

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null || userRole.NormalizedName == "ADMIN" || userRole.Id == 1)
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Role",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                ImageSrc = "default.jpeg",
                FullName = model.Username

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);

                if (result.Succeeded)
                {
                    try
                    {
                        if(ImageHelper.IsValidImageType(model.Image))
                        {
                            Debug.Assert(model.Image != null);

                            var extension = Path.GetExtension(model.Image.FileName).ToLower();
                            extension = (extension == ".jpeg" || extension == ".jpg") ?
                                            extension : ImageHelper.GetImageExtenstion(model.Image.ContentType);

                            var imageURL = "user_" + user.Id + "." + extension;

                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),
                                                    "uploads", "images", "users");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var filePath = Path.Combine(uploadsFolder, imageURL);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await model.Image.CopyToAsync(stream);
                            }

                            await _appUserRepo.UpdateUserImage(user, imageURL);
                        }

                        return Results.Ok(new SuccessResponse()
                        {
                            Message = "User created successfully",
                            Data = new AppUserDTO()
                            {
                                Email = user.Email,
                                PhoneNumber = user.PhoneNumber ?? "",
                                Id = user.Id,
                                PhoneRegionCode = user.PhoneRegionCode,
                                UserName = user.FullName,
                                Image = new ImageMetadata() 
                                {
                                    ImageURL = "https://localhost/api/users/image",
                                    Name = user.ImageSrc 
                                }
                            },
                            Code = System.Net.HttpStatusCode.OK
                        });
                    }
                    catch (Exception)
                    {
                        return Results.BadRequest(new ErrorResponse()
                        {
                            Message = "Couldn't update user image",
                            Code = System.Net.HttpStatusCode.BadRequest,
                        });
                    }
                }
            }

            return Results.BadRequest(new ErrorResponse()
            {
                Message = result.Errors,
                Code = System.Net.HttpStatusCode.BadRequest,
            });
        }

        
        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {
            var user = await _appUserRepo.GetUserWithTokenAndRoles(model.Email);
            if (user == null)
                return Results.NotFound(new ErrorResponse 
                { 
                    Message = "Email does not exist",
                    Code = System.Net.HttpStatusCode.NotFound
                });

            if(user.RefreshToken != null)
                _tokenBlacklistService.BlacklistToken
                    (user.RefreshToken.LatestJwtAccessTokenJti, user.RefreshToken.LatestJwtAccessTokenExpiry);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest(new ErrorResponse 
                { 
                    Message = "User is LockedOut Try again in 5 minutes" ,
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

                (string accessToken, string jti) = _tokenService.GenerateJwtToken(user);

                refreshToken.LatestJwtAccessTokenJti = jti;
                refreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                    (double.Parse(_jwtOptions.AccessTokenValidityMinutes));

                var res = await _appUserRepo.UpdateRefreshToken(user, refreshToken);

                if (res == false)
                    return Results.BadRequest(new ErrorResponse()
                    {
                        Message = "Couldn't SignIn",
                        Code = System.Net.HttpStatusCode.BadRequest
                    });

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true ,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
                };

                httpContext.Response.Cookies.Append("refreshToken", raw, cookieOptions);

                string imagePath = Path.Combine(Directory.GetCurrentDirectory(),
                                                "uploads", "images", user.ImageSrc);


                return Results.Ok(new SuccessResponse()
                {
                    Message = "User Logged In successfully",
                    Data = new
                    {
                        user.Id,
                        user.Banned,
                        Name = user.FullName,
                        Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                        ValidTo = DateTime.UtcNow.AddMinutes(
                            double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture),
                        Image = new 
                        { 
                            Url = "https://localhost/api/users/image",
                            Name = user.ImageSrc
                        },
                        AccessToken = accessToken,

                    },
                    Code = System.Net.HttpStatusCode.OK
                });

            }

            await _userManager.AccessFailedAsync(user);
            return Results.BadRequest(new ErrorResponse
            {
                Message = "Password is incorrect!",
                Code = System.Net.HttpStatusCode.BadRequest
            });
        }


        public async Task<IResult> Logout(HttpContext httpContext)
        {
            string? refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Results.BadRequest(new ErrorResponse() 
                {
                    Message = "RefreshToken is not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                }); 

            string? accessToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (accessToken is null)
                return Results.BadRequest(new ErrorResponse() { 
                    Message = "Jwt Token not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            int id; string accessJti;
            try
            {
                (id, accessJti) = _tokenService.ExtractIdAndJtiFromExpiredToken(accessToken); // allow expired token to signout
            }
            catch
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Invalid AccessToken",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }


            var dbRefreshToken = await _appUserRepo.GetUserRefreshToken(id);
            if (dbRefreshToken is null)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "User is not Signed In",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            if (!_tokenService.VerifyRefreshHmac(refreshToken, dbRefreshToken.EncryptedToken))
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Invalid RefreshToken !",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            if (accessJti != dbRefreshToken.LatestJwtAccessTokenJti)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Latest AccessToken Doesn't match",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            // Get Latest Access Token (from database) and Blacklist it if it's the one sent else ignore it
            _tokenBlacklistService.BlacklistToken
                (dbRefreshToken.LatestJwtAccessTokenJti, dbRefreshToken.LatestJwtAccessTokenExpiry);
            var res = await _appUserRepo.DeleteRefreshToken(id);

            if(res == false)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "User Does not exist",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.Now.AddDays(-1)
            };

            httpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);

            return Results.Ok(new SuccessResponse
            {
                Message = "User Logged Out successfully",
                Code = System.Net.HttpStatusCode.OK
            });

        }
    }
}
