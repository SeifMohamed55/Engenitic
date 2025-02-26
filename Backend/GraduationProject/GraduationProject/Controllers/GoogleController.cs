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
using Microsoft.AspNetCore.Http.HttpResults;
using GraduationProject.Services;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly MailingOptions _options;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILoginRegisterService _loginService;
        public GoogleController(
            IOptions<MailingOptions> options,
            UserManager<AppUser> userManager,
            ILoginRegisterService loginService
            ) 
        {
            _options = options.Value;
            _userManager = userManager;
            _loginService = loginService;
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




        [HttpGet("login")]
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
        public async Task<IResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return Results.BadRequest(); // Handle error

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;

            var accessToken = authenticateResult.Properties.GetTokenValue("access_token");
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // Unique Google ID
            string? image = null;
            
            using (var client = new HttpClient())
            {
                var res = await client.GetFromJsonAsync<GoogleProfileResponse>
                        ($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");

                image = res?.Picture;
            }

            if (email == null || name == null || image == null || googleId == null)
                return Results.BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "email or name or photo does not exist"
                });

            AuthenticatedPayload payload = new()
            {
                 Email = email,
                 Name =  name,
                 UniqueId = googleId,
                 Image = image,
            };


            return await _loginService.ExternalLogin(GoogleDefaults.DisplayName, payload, HttpContext);

        }


        [HttpGet("AccessDeniedPathInfo")]
        public IActionResult AccessDenied()
        {
            return Unauthorized(new ErrorResponse()
            {
                Code = System.Net.HttpStatusCode.Unauthorized,
                Message = "Authentication Failed"
            });
        }






    }
}
