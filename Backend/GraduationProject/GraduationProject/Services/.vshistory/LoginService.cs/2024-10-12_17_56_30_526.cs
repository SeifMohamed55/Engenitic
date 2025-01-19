using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpringBootCloneApp.Controllers.APIResponses;
using SpringBootCloneApp.Controllers.RequestModels;
using SpringBootCloneApp.Controllers.ResponseModels;
using SpringBootCloneApp.Models.Enums;
using SpringBootCloneApp.Models;
using SpringBootCloneApp.StartupConfigurations;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using SpringBootCloneApp.Data;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;

namespace SpringBootCloneApp.Services
{
    public interface ILoginService
    {
        Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload , HttpContext httpContext);
        Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext);
        IResult Logout(HttpContext httpContext);
    }
    public class LoginService : ILoginService
    {
        private readonly UserManager<Client> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly AppDbContext _context;
        private readonly SignInManager<Client> _signInManager;
        private readonly JwtOptions _jwtOptions;
        private readonly ICachingService _cachingService;
        public LoginService(
            UserManager<Client> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            SignInManager<Client> signInManager,
            AppDbContext context,
            ICachingService cachingService
            )

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _context = context;
            _signInManager = signInManager;
            _cachingService = cachingService;
        }
        public async Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload, HttpContext httpContext)
        {
            var userInDatabase = await _context.Clients
                .Include(x => x.Authorities)
                .FirstOrDefaultAsync(x => x.Email == payload.Email);


            if (userInDatabase == null)
            {
                Client client = new Client()
                {

                    Email = payload.Email,
                    Address = "",
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    UserName = payload.Email,
                };
                client.PasswordHash = _userManager.PasswordHasher.HashPassword(client, Guid.NewGuid().ToString());
                var result = await _userManager.CreateAsync(client);

                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(client, Role.ROLE_USER.ToString());
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

            var user = await _context.Clients
                       .Include(x => x.Authorities)
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

            httpContext.Response.Cookies.Append("refreshToken", refreshToken.Value, cookieOptions);

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

        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {
            var user = await _context.Clients
               .Include(x => x.Authorities)
               .FirstOrDefaultAsync(x => x.Email == model.Email);

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

                httpContext.Response.Cookies.Append("refreshToken", refreshToken.Value, cookieOptions);


                return Results.Ok(new RefreshTokenResponse()
                {
                    AccessToken = accessToken,
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidity)).ToString("f")
                });
            }
            return Results.NotFound("Either Email or Password is incorrect!");
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
