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
        private readonly AppUsersRepository _appUserRepo;
        private readonly ITokenBlacklistService _tokenBlacklistService;

        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            SignInManager<AppUser> signInManager,
            AppDbContext context,
            RoleManager<Role> roleManager,
            AppUsersRepository appUsersRepository,
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


        private bool IsValidImageType(IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return false;
            else
            {
                var allowedExtensions = new HashSet<string>
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff"
                };


                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(image.FileName, out var contentType))
                {
                    return false;
                }
                var allowedMimeTypes = new HashSet<string>
                {
                    "image/jpeg", "image/png", "image/gif", "image/bmp",
                    "image/webp", "image/tiff"
                };

                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension) || !allowedMimeTypes.Contains(contentType))
                {
                    return false;
                }

                long maxFileSize = 2 * 1024 * 1024; // 2MB
                if (image.Length > maxFileSize)
                {
                    return false;
                }
                return true;
            }
        }


        private string GetImageType(string fileExtension)
        {
           return fileExtension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream", // default MIME type if unknown
            };
        }

        public async Task<IResult> Register(RegisterCustomRequest model)
        {
            if(await _userManager.FindByEmailAsync(model.Email) != null)
                return Results.BadRequest("Email already exists");

            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" && model.Role != "student")
                return Results.BadRequest("Invalid Role!");

            if (model.Password != model.ConfirmPassword)
            {
                return Results.BadRequest("Passwords do not match");
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
                    return Results.BadRequest("Invalid phone number");
                }
            }

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null || userRole.NormalizedName == "ADMIN" || userRole.Id == 1)
                return Results.BadRequest();

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
                ImageURL = "default.jpeg",
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
                        if(IsValidImageType(model.Image))
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

                            user.ImageURL = imageURL;
                            await _appUserRepo.UpdateAsync(user);
                        }

                        return Results.Ok(new
                        {
                            result = "User created successfully",
                            user = new AppUserDto()
                            {
                                Email = user.Email,
                                PhoneNumber = user.PhoneNumber ?? "",
                                Id = user.Id,
                                PhoneRegionCode = user.PhoneRegionCode,
                                UserName = user.FullName
                            }
                        });
                    }
                    catch (Exception)
                    {
                        return Results.BadRequest("Couldn't update user image");
                    }
                }
            }

            return Results.BadRequest(result.Errors);
        }

        
        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {

            var user = await _appUserRepo.GetUserWithTokenAndRoles(model.Email);
            if (user == null)
                return Results.NotFound("Email does not exist");

            if(user.RefreshToken != null)
                _tokenBlacklistService.BlacklistToken(user.RefreshToken.LatestJwtAccessToken);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest("LockedOut");

            if (result.Succeeded)
            {

                (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

                var accessToken = _tokenService.GenerateJwtToken(user);

                refreshToken.LatestJwtAccessToken = accessToken;

                var res = await _appUserRepo.UpdateRefreshToken(user, refreshToken);

                if (res == false)
                    return Results.BadRequest("Couldn't SignIn");

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true ,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
                };

                httpContext.Response.Cookies.Append("refreshToken", raw, cookieOptions);

                string imagePath = Path.Combine(Directory.GetCurrentDirectory(),
                                                "uploads", "images", user.ImageURL);

                try
                {
                    var imageBytes = File.ReadAllBytes(imagePath);
                    var base64Image = Convert.ToBase64String(imageBytes);
                    var fileExtension = Path.GetExtension(imagePath).ToLower();
                    var imageSrc = $"data:{GetImageType(fileExtension)};base64,{base64Image}";

                    return Results.Ok(new
                    {
                        User = new
                        {
                            Name = user.FullName,
                            Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                            AccessToken = accessToken,
                            ValidTo = DateTime.UtcNow.AddMinutes(
                                double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                        .ToString("f", CultureInfo.InvariantCulture),
                            Image = imageSrc,
                        }
                    });
                }
                catch (Exception)
                {
                    Results.BadRequest("Directory Not Found");
                }
            }
            return Results.NotFound("Password is incorrect!");
        }


        public async Task<IResult> Logout(HttpContext httpContext)
        {
            string? refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Results.BadRequest("RefreshToken is not found"); 

            string? accessToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (accessToken is null)
                return Results.BadRequest("Jwt Token not found");


            int? id = _tokenService.ExtractIdFromExpiredToken(accessToken); // allow expired token to signout

            if (!id.HasValue)
                return Results.BadRequest("Invalid AccessToken");

            var dbRefreshToken = await _appUserRepo.GetUserRefreshToken(id.Value);
            if (dbRefreshToken is null)
                return Results.BadRequest("User is not Signed In");

            if (!_tokenService.VerifyRefreshHmac(refreshToken, dbRefreshToken.EncryptedToken))
                return Results.BadRequest("Invalid RefreshToken !");

            if (accessToken != dbRefreshToken.LatestJwtAccessToken)
                return Results.BadRequest("Latest AccessToken Doesn't match");

            // Get Latest Access Token (from database) and Blacklist it if it's the one sent else ignore it
            _tokenBlacklistService.BlacklistToken(dbRefreshToken.LatestJwtAccessToken);
            var res = await _appUserRepo.DeleteRefreshToken(id.Value);

            if(res == false)
                return Results.BadRequest("User Does not exist");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(-1)
            };

            httpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);

            return Results.Ok();

        }
    }
}
