using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.Controllers.APIResponses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Microsoft.Extensions.Options;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using GraduationProject.Models;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly MailingOptions _options;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        public GoogleController(
            IOptions<MailingOptions> options,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager
            ) 
        {
            _options = options.Value;
            _signInManager = signInManager;
            _userManager = userManager;
        }

       
        [HttpGet("GetGmailRefreshToken")]
        [Authorize(Roles ="admin")]
        public IActionResult CreateAuthorizationCode() 
        {
            var authorizationUrl = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret
                },
                Scopes = [GmailService.Scope.GmailSend] ,
                Prompt = "consent",
            }).CreateAuthorizationCodeRequest("https://localhost:443/api/google/GetGmailRefreshTokenCallback")
            .Build();

            return Ok(new SuccessResponse()
            {
               Message = "Redirect to this link",
               Data = authorizationUrl,
               Code = System.Net.HttpStatusCode.OK
            });
        }


        [HttpGet("GetGmailRefreshTokenCallback")]
        public async Task<IActionResult> GetGmailRefreshTokenCallback([FromQuery] string code)
        {
            var tokenResponse = await new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret
                },
                Scopes = [GmailService.Scope.GmailSend],
                Prompt = "consent",
            }).ExchangeCodeForTokenAsync(
                _options.Email,
                code,
                "https://localhost:443/api/google/GetGmailRefreshTokenCallback",
                CancellationToken.None
            );


            var refreshToken = tokenResponse.RefreshToken;
            if(refreshToken == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "No refresh token."
                });


            var user = await _userManager.FindByEmailAsync(_options.Email);
            if (user == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "User not found"
                });


            var res1 = await _userManager.SetAuthenticationTokenAsync(user, "Google", "refresh_token", refreshToken);

            if (res1.Succeeded)
                return Ok(new SuccessResponse() 
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "Gmail Refresh token updated Successfully"
                });
            
            return BadRequest(new ErrorResponse()
            {
                Code = System.Net.HttpStatusCode.BadRequest,
                Message = "No Refresh Token"
            });

        }




/*        [HttpGet("login")]
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        // Handle the callback from Google
        [HttpGet("Callback")]
        public async Task<IActionResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest(); // Handle error

            // Extract tokens from the authentication properties
            var authProperties = authenticateResult.Properties;
            var accessToken = authProperties.GetTokenValue("access_token");
            var refreshToken = authProperties.GetTokenValue("refresh_token");

            // Store the refresh token securely (e.g., in a database)
            // For example:
            // _userService.SaveRefreshToken(User.FindFirstValue(ClaimTypes.NameIdentifier), refreshToken);

            // Extract user information
            var claims = authenticateResult.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Sign in the user
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok("Done");
        }


        [HttpGet("AccessDeniedPathInfo")]
        public IActionResult AccessDenied()
        {
            return Unauthorized(new ErrorResponse()
            {
                Code = System.Net.HttpStatusCode.Unauthorized,
                Message = "Authentication Failed"
            });
        }*/






    }
}
