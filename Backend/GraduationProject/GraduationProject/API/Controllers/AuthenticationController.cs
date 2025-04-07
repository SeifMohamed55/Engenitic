using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GraduationProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoginRegisterService _loginService;
        private readonly JwtOptions _jwtOptions;
        public AuthenticationController(ILoginRegisterService loginService, IOptions<JwtOptions> options)
        {
            _loginService = loginService;
            _jwtOptions = options.Value;
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
                (ServiceResult<LoginResponse> res, string? rawRefreshToken) = await _loginService.Login(model);

                if (res.IsSuccess && rawRefreshToken != null)
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
                    };

                    HttpContext.Response.Cookies.Append("refreshToken", rawRefreshToken, cookieOptions);

                    return Ok(new SuccessResponse()
                    {
                        Code = System.Net.HttpStatusCode.OK,
                        Data = res.Data,
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


        // not authorized endpoint
        [HttpPost("logout")]
        public async Task<IActionResult> Revoke()
        {
            string? refreshToken = HttpContext.Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "RefreshToken is not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            string? accessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (accessToken is null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Jwt Token not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            var res = await _loginService.Logout(accessToken, refreshToken);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.Now.AddDays(-1)
            };

            HttpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);

            if (res.IsSuccess)           
                return Ok(new SuccessResponse()
                {
                    Code = System.Net.HttpStatusCode.OK,
                    Message = res.Data ?? "User logged out successfully."
                });
            
            else           
                return BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = res.Message ?? "Couldn't logout user."
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

