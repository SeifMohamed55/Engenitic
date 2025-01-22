using Ganss.Xss;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GraduationProject.Services
{
    public interface ILoginRegisterService
    {
        Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload , HttpContext httpContext);
        Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext);
        IResult Logout(HttpContext httpContext);

        Task<IResult> Register(RegisterCustomRequest model);
    }
    public class LoginRegisterService : ILoginRegisterService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly AppDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ICachingService _cachingService;
        private readonly RoleManager<Role> _roleManager;
        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            SignInManager<AppUser> signInManager,
            AppDbContext context,
            ICachingService cachingService,
            RoleManager<Role> roleManager
            )

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _context = context;
            _signInManager = signInManager;
            _cachingService = cachingService;
            _roleManager = roleManager;
        }

        public async Task<IResult> Register(RegisterCustomRequest model)
        {
            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" || model.Role != "student")
                return Results.BadRequest();

            if (model.Password != model.ConfirmPassword)
            {
                return Results.BadRequest("Passwords do not match");
            }

            string? RegionCode = null;
            if (model.PhoneNumber != null)
            {
                (string, string)? phoneDetails = Utilities.IsValidPhoneNumber(model.PhoneNumber);
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
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                imageURL = model.imageURL,

            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);

                if (result.Succeeded)
                    return Results.Ok(new
                    {
                        result = "User created successfully",
                        user = new AppUserDTO()
                        {
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber ?? "",
                            Id = user.Id,
                            imageURL = user.imageURL,
                            RegionCode = user.PhoneRegionCode
                        }
                    });
            }

            return Results.BadRequest(result.Errors);
        }

        
        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Results.NotFound("Either Email or password does not exist");

            var result = await _signInManager
                .PasswordSignInAsync(user.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest("LockedOut");

            if (result.Succeeded)
            {
                var accessToken = _tokenService.GenerateSymmetricJwtToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _cachingService.AddRefreshTokenCachedData(accessToken, refreshToken);


                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true ,
                    SameSite = SameSiteMode.Strict,
                };

                httpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);


                return Results.Ok(new RefreshTokenResponse()
                {
                    AccessToken = accessToken,
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes)).ToString("f")
                });
            }
            return Results.NotFound("Either Email or Password is incorrect!");
        }


        public async Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload, HttpContext httpContext)
        {
            var userInDatabase = await _context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == payload.Email);


            if (userInDatabase == null)
            {
                AppUser client = new AppUser()
                {
                    Email = payload.Email,
                    UserName = payload.GivenName + " " + payload.FamilyName
                };

                client.PasswordHash = _userManager.PasswordHasher.HashPassword(client, Guid.NewGuid().ToString());
                var result = await _userManager.CreateAsync(client);

                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(client, "USER");
                    if (!result.Succeeded)
                        return Results.BadRequest(result.Errors);

                    var loginInfo = new UserLoginInfo
                        (provider, payload.Id, provider);

                    result = await _userManager.AddLoginAsync(client, loginInfo);
                    if (!result.Succeeded)
                        return Results.BadRequest(result.Errors);
                }
                else
                    return Results.BadRequest(result.Errors);
            }

            var user = await _context.Users
                       .Include(x => x.Roles)
                       .FirstAsync(x => x.Email == payload.Email);

            var providerLogin = _context.UserLogins.Where(x => x.UserId == user.Id)
            .Select(x => x.LoginProvider)
                .Contains(provider);

            if (!providerLogin)
            {
                var loginInfo = new UserLoginInfo
                       (provider, payload.Id, provider);
                var result = await _userManager.AddLoginAsync(user, loginInfo);
                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors);

            }

            var accessToken = _tokenService.GenerateSymmetricJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _cachingService.AddRefreshTokenCachedData(accessToken, refreshToken);

            var signInResult = await _signInManager.ExternalLoginSignInAsync
                (provider, payload.Id, false);

            if (!signInResult.Succeeded)
                return Results.BadRequest("Couldn't SignIn");

            // httpContext.Response.Cookies.Delete("refreshToken");

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            httpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            var htmlContent = $@"
                <html>
                <head>
                    <title>Authentication Successful</title>
                </head>
                <body>
                    <script>
                        (function() {{
                            function sendToken() {{
                                window.opener.postMessage({{ type: 'jwt', jwt: '{accessToken}' }}, 'http://127.0.0.1:3000');
                                window.close();
                            }}
                            sendToken();
                        }})();
                    </script>
                    <p>Authentication successful. You can close this window.</p>
                </body>
                </html>
            ";

            return Results.Content(htmlContent, "text/html");
        }


        public IResult Logout(HttpContext httpContext)
        {
            if (httpContext.Request.Cookies["refreshToken"] is null)
                return Results.BadRequest(); 

            string? accessToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (accessToken is null)
                return Results.BadRequest();

            _cachingService.RemoveCachedRefreshToken(accessToken);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            httpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);
            //httpContext.Response.Cookies.Delete("refreshToken");
            return Results.NoContent();
        }

    }
}
