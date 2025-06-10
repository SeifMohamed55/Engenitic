using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace GraduationProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly JwtOptions _jwtOptions;
        private readonly IJwtTokenService _jwtTokenService;
        public AuthenticationController(IAuthenticationService loginService, IOptions<JwtOptions> options, IJwtTokenService jwtTokenService)
        {
            _authenticationService = loginService;
            _jwtOptions = options.Value;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        //[EnableRateLimiting("UserLoginRateLimit")]
        public async Task<IActionResult> Login(LoginCustomRequest model)
        {
            try
            {

                if (Guid.TryParse(HttpContext.Request.Cookies["device_id"], out var guid))
                {
                    model.DeviceId = guid;
                }
                else
                {
                    model.DeviceId = Guid.NewGuid();
                }

                // (for IP) HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                // At production if behind a proxy

                var deviceInfo = new DeviceInfo
                {
                    DeviceId = model.DeviceId.Value,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown"
                };

                ServiceResult<LoginWithCookies> res = await _authenticationService.Login(model, deviceInfo);

                if (res.TryGetData(out var data))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        IsEssential = true
                    };

                    if (model.RememberMe)
                        cookieOptions.Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays));

                    HttpContext.Response.Cookies.Append("refreshToken", data.RefreshToken.Token.ToString(), cookieOptions);

                    cookieOptions.Expires = DateTime.UtcNow.AddDays(30);
                    HttpContext.Response.Cookies.Append("device_id", data.RefreshToken.DeviceId.ToString(), cookieOptions);


                    return Ok(new SuccessResponse()
                    {
                        Data = data.LoginResponse,
                        Message = "User logged in successfully."
                    });

                }
                else
                    return res.ToActionResult();
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Couldn't login user."
                });
            }
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Revoke()
        {
            var deviceId = HttpContext.Request.Cookies["device_id"];
            var jwt = _jwtTokenService.ExtractJwtTokenFromContext(HttpContext);


            if (Guid.TryParse(deviceId, out var guid) && jwt != null)
            {
                (int userId, string jti) = _jwtTokenService.ExtractIdAndJtiFromExpiredToken(jwt);

                var res = await _authenticationService.Logout(guid, userId);

                if (!res.TryGetData(out var data))
                    return Ok(new SuccessResponse()
                    {
                        Message = "User logged out successfully."
                    });

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    IsEssential = true
                };
                if (data.RememberMe)
                {
                    cookieOptions.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);
                }
                else
                {
                    Response.Cookies.Delete("refreshToken", cookieOptions);
                }

                return Ok(new SuccessResponse()
                {
                    Message = "User logged out successfully."
                });
            }
            else
                return Ok(new SuccessResponse()
                {
                    Message = "User logged out successfully."
                });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterCustomRequest model)
        {

            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" && model.Role != "student")
                return BadRequest(new ErrorResponse()
                {
                    Message = "Role must be either 'instructor' or 'student'."
                });


            var res = await _authenticationService.Register(model, false);
            return res.ToActionResult();
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest model)
        {
            var res = await _authenticationService.ForgetPassword(model);
            return res.ToActionResult();

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            var res = await _authenticationService.ResetPassword(model);
            return res.ToActionResult();
        }

    }
}

