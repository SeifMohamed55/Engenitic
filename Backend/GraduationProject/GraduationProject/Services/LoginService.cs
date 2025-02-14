using Ganss.Xss;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Diagnostics;

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

            HtmlSanitizer sanitizer = new HtmlSanitizer();
            model.Username = sanitizer.Sanitize(model.Username);
            model.Email = sanitizer.Sanitize(model.Email);
            model.PhoneNumber = sanitizer.Sanitize(model.PhoneNumber ?? "");

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

                            var imageURL = "user_" + user.Id + ".jpeg";

                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),
                                                    "uploads", "images");
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
                                ImageURL = "https://localhost/api/users/image"
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
                _tokenBlacklistService.BlacklistToken(user.RefreshToken.LatestJwtAccessToken);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest(new ErrorResponse 
                { 
                    Message = "User is LockedOut" ,
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            if (result.Succeeded)
            {

                (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

                var accessToken = _tokenService.GenerateJwtToken(user);

                refreshToken.LatestJwtAccessToken = accessToken;

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
                        Name = user.FullName,
                        Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                        AccessToken = accessToken,
                        ValidTo = DateTime.UtcNow.AddMinutes(
                            double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture),
                        ImageLink = "https://localhost/api/users/image",
                    },
                    Code = System.Net.HttpStatusCode.OK
                });

            }
            return Results.NotFound(new ErrorResponse
            {
                Message = "Password is incorrect!",
                Code = System.Net.HttpStatusCode.NotFound
            });
        }


        // U can make latest access token with null and don't use blacklisting
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


            int? id = _tokenService.ExtractIdFromExpiredToken(accessToken); // allow expired token to signout

            if (!id.HasValue)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Invalid AccessToken",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            var dbRefreshToken = await _appUserRepo.GetUserRefreshToken(id.Value);
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

            if (accessToken != dbRefreshToken.LatestJwtAccessToken)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Latest AccessToken Doesn't match",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            // Get Latest Access Token (from database) and Blacklist it if it's the one sent else ignore it
            _tokenBlacklistService.BlacklistToken(dbRefreshToken.LatestJwtAccessToken);
            var res = await _appUserRepo.DeleteRefreshToken(id.Value);

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
