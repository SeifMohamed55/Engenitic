using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GraduationProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("UserLoginRateLimit")]
    public class TokenController : ControllerBase
    {
        private readonly IRefreshTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;

        public TokenController(IRefreshTokenService tokenService, IOptions<JwtOptions> options)
        {
            _tokenService = tokenService;
            _jwtOptions = options.Value;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // add latest true accesstoken to database and check it
            var requestRefToken = HttpContext.Request.Cookies["refreshToken"];
            if (requestRefToken == null)
                return BadRequest(new ErrorResponse
                {
                    Message = "No Refresh Token Provided.",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            string? oldAccessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (oldAccessToken == null)
                return BadRequest(new ErrorResponse
                {
                    Message = "No Access Token Provided.",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            var result = await _tokenService.Refresh(oldAccessToken, requestRefToken);

            if (result.IsSuccess && result.Data != null)
                return Ok(new SuccessResponse
                {
                    Message = "Token Refreshed Successfully",
                    Code = System.Net.HttpStatusCode.OK,
                    Data = new AccessTokenResponse
                    {
                        AccessToken = result.Data,
                        ValidTo = DateTime.UtcNow
                                    .AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture)
                    }
                });
            else
                return BadRequest(new ErrorResponse
                {
                    Message = result.Error ?? "",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

        }

    }
}
