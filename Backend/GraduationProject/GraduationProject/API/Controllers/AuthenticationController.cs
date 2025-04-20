using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
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
        private readonly ILoginRegisterService _loginService;
        private readonly JwtOptions _jwtOptions;
        private readonly IJwtTokenService _jwtTokenService;
        public AuthenticationController(ILoginRegisterService loginService, IOptions<JwtOptions> options, IJwtTokenService jwtTokenService)
        {
            _loginService = loginService;
            _jwtOptions = options.Value;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        //[EnableRateLimiting("UserLoginRateLimit")]
        public async Task<IActionResult> Login(LoginCustomRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = string.Join('/', ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)),
                });
            try
            {

                if(Guid.TryParse(HttpContext.Request.Cookies["device_id"], out var guid))
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
                    DeviceId =  model.DeviceId.Value,
                    IpAddress =  HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown"
                };

                ServiceResult<LoginWithCookies> res = await _loginService.Login(model, deviceInfo);

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
                        Code = System.Net.HttpStatusCode.OK,
                        Data = data.LoginResponse,
                        Message = "User logged in successfully."
                    });

                }
                else
                    return BadRequest(new ErrorResponse()
                    {
                        Code = System.Net.HttpStatusCode.BadRequest,
                        Message = res.Message ?? "Couldn't login user."
                    });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
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

                var res = await _loginService.Logout(guid, userId);

                if(!res.TryGetData(out var data))
                    return Ok(new SuccessResponse()
                    {
                        Code = System.Net.HttpStatusCode.OK,
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
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "User logged out successfully."
                });
            }
            else
                return Ok(new SuccessResponse()
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "User logged out successfully."
                });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterCustomRequest model)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = ModelState,
                });

            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" && model.Role != "student")
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Role must be either 'instructor' or 'student'."
                });


            var res = await _loginService.Register(model, false);
            if (res.IsSuccess)
                return Ok(new SuccessResponse()
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Data = res.Data,
                    Message = "User registered successfully."
                });
            else
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = res.Message ?? "Couldn't register user."
                });

        }

    }
}

