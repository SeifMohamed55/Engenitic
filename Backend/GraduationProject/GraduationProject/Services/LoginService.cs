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
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

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
            model.imageURL = "images/default.jpg";

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                imageURL = model.imageURL,
                FullName = model.Username

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);

                if (result.Succeeded)
                    return Results.Ok(new
                    {
                        result = "User created successfully",
                        user = new AppUserDto()
                        {
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber ?? "",
                            Id = user.Id,
                            ImageURL = user.imageURL,
                            PhoneRegionCode = user.PhoneRegionCode,
                            UserName = user.FullName
                        }
                    });
            }

            return Results.BadRequest(result.Errors);
        }

        
        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Results.NotFound("Email does not exist");

            var result = await _signInManager
                .PasswordSignInAsync(user.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest("LockedOut");

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                user.Roles = roles.Select(x=> new Role(){ Name = x}).ToList();
                var accessToken = _tokenService.GenerateJwtToken(user);
                (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

                var res = await _appUserRepo.UpdateRefreshToken(user, refreshToken);
                if(res == false)
                    return Results.BadRequest("Couldn't SignIn");

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true ,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
                };

                httpContext.Response.Cookies.Append("refreshToken", raw, cookieOptions);


                return Results.Ok(new AccessTokenResponse()
                {
                    AccessToken = accessToken,
                    ValidTo = DateTime.UtcNow.AddMinutes(
                        double.Parse(_jwtOptions.AccessTokenValidityMinutes)).ToString("f", CultureInfo.InvariantCulture)
                });
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

            _tokenBlacklistService.BlacklistToken(accessToken);

            int? id = _tokenService.ExtractIdFromExpiredToken(accessToken); // allow expired token to signout

            if (!id.HasValue)
                return Results.BadRequest("Invalid AccessToken");

            var dbRefreshToken = await _appUserRepo.GetUserRefreshToken(id.Value);
            if (dbRefreshToken is null)
                return Results.BadRequest("User is not Signed In");

            if (!_tokenService.VerifyRefreshHmac(refreshToken, dbRefreshToken))
                return Results.BadRequest("Invalid RefreshToken !");

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
