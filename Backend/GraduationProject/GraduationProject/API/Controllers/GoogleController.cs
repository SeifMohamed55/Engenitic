using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using System.Web;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly MailingOptions _options;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILoginRegisterService _loginService;
        private readonly JwtOptions _jwtOptions;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public GoogleController(
            IOptions<MailingOptions> options,
            UserManager<AppUser> userManager,
            ILoginRegisterService loginService,
            IOptions<JwtOptions> jwtOptions,
            ICloudinaryService cloudinaryService,
            IOptions<JsonOptions> jsonOptions
            )
        {
            _options = options.Value;
            _userManager = userManager;
            _loginService = loginService;
            _jwtOptions = jwtOptions.Value;
            _cloudinaryService = cloudinaryService;
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        }


        [HttpGet("GetGmailRefreshToken")]
        [Authorize(Roles = "admin")]
        public IActionResult CreateAuthorizationCode()
        {
            var authorizationUrl = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret
                },
                Scopes = [GmailService.Scope.GmailSend],
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
            if (refreshToken == null)
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
        public async Task<IActionResult> Callback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return GetHtmlContent(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Authentication failed"
                }); // Handle error

            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;

            var accessToken = authenticateResult.Properties.GetTokenValue("access_token");
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // Unique Google ID
            string? image = null;


            using (var client = new HttpClient())
            {
                var response = await client.GetFromJsonAsync<GoogleProfileResponse>
                        ($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");

                image = response?.Picture;
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            if (email == null || name == null || googleId == null)
                return GetHtmlContent(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "email or name or photo does not exist"
                });

            if(image == null)
                image = ICloudinaryService.DefaultUserImagePublicId;

            AuthenticatedPayload payload = new()
            {
                Email = email,
                Name = name,
                UniqueId = googleId,
                ImageUrl = image,
            };
            try
            {
                (ServiceResult<LoginResponse> res, string? rawRefreshToken) = await _loginService.ExternalLogin(GoogleDefaults.DisplayName, payload);
                if (res.IsSuccess && rawRefreshToken != null)
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        IsEssential = true
                    };

                    HttpContext.Response.Cookies.Append("refreshToken", rawRefreshToken, cookieOptions);

                    return GetHtmlContent(new SuccessResponse()
                    {
                        Code = System.Net.HttpStatusCode.OK,
                        Data = res.Data,
                        Message = "User logged in successfully."
                    });

                }
                else
                    return GetHtmlContent(new ErrorResponse()
                    {
                        Code = System.Net.HttpStatusCode.BadRequest,
                        Message = res.Message ?? "Couldn't login user."
                    });
            }
            catch
            {
                return GetHtmlContent(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Couldn't login"
                });
            }

        }

        private ContentResult GetHtmlContent(IResponse response)
        {
            var errorResp = response as ErrorResponse;
            var successResp = response as SuccessResponse;

            string serializedResponse;

            if(errorResp != null)
                serializedResponse = JsonSerializer.Serialize(errorResp, _jsonSerializerOptions);
            else
                serializedResponse = JsonSerializer.Serialize(successResp, _jsonSerializerOptions);

            var jsonResponse = HttpUtility.JavaScriptStringEncode(serializedResponse);


            var html = $@"
                 <html>
                 <head>
                     <title>Authentication Successful</title>
                 </head>
                 <body>
                     <script>
                         (function() {{
                             function sendToken() {{
                                 var data = JSON.parse(""{jsonResponse}""); // ✅ Safely parse JSON
                                 window.opener.postMessage(data, '{_jwtOptions.Audience}');
                                 window.close();
                             }}
                             sendToken();
                         }})();
                     </script>
                     <p>Authentication successful. You can close this window.</p>
                 </body>
                 </html>";

            return Content(html, "text/html");

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
