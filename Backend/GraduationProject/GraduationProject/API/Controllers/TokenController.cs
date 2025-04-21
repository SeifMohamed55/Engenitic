using Google.Apis.Auth.OAuth2.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
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
            if (string.IsNullOrEmpty(requestRefToken))
                return Unauthorized(new ErrorResponse
                {
                    Message = "No Refresh Token Provided.",
                    Code = System.Net.HttpStatusCode.Unauthorized
                });

            string? oldAccessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(oldAccessToken))
                return Unauthorized(new ErrorResponse
                {
                    Message = "No Access Token Provided.",
                    Code = System.Net.HttpStatusCode.Unauthorized
                });

            string? deviceId = HttpContext.Request.Cookies["device_id"];
            if (string.IsNullOrEmpty(deviceId))
                return Unauthorized(new ErrorResponse
                {
                    Message = "No Device ID Provided.",
                    Code = System.Net.HttpStatusCode.Unauthorized
                });

            if(!Guid.TryParse(deviceId, out var guid))
                return Unauthorized(new ErrorResponse
                {
                    Message = "Provided DeviceID is Invalid .",
                    Code = System.Net.HttpStatusCode.Unauthorized
                });

            var result = await _tokenService.Refresh(oldAccessToken, requestRefToken, guid);

            if (result.TryGetData(out var data))
                return Ok(new SuccessResponse
                {
                    Message = "Token Refreshed Successfully",
                    Code = System.Net.HttpStatusCode.OK,
                    Data = new AccessTokenResponse
                    {
                        AccessToken = data,
                        ValidTo = DateTime.UtcNow
                                    .AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture)
                    }
                });
            else
                return Unauthorized(new ErrorResponse
                {
                    Message = result.Message ?? "",
                    Code = System.Net.HttpStatusCode.Unauthorized
                });

        }

    }
}
