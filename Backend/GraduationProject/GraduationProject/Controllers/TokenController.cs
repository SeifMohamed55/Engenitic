using GraduationProject.Controllers.APIResponses;
using GraduationProject.Repositories;
using GraduationProject.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly IUserRepository _appUsersRepository;
        private readonly IEncryptionService _encryptionService;
        public TokenController
            (IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            IUserRepository appUsersRepository,
            IEncryptionService encryptionService)
        {
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _appUsersRepository = appUsersRepository;
            _encryptionService = encryptionService;
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

            if(_tokenService.IsAccessTokenValid(oldAccessToken))
                return BadRequest(new ErrorResponse
                {
                    Message = "AccessToken is valid",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            int extractedId;
            try
            {
                extractedId = _tokenService.ExtractIdFromExpiredToken(oldAccessToken);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse 
                {
                    Message = "Provided token is invalid",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });
            }


            var user = await _appUsersRepository.GetUserWithTokenAndRoles(extractedId);

            if (user == null || 
                user.RefreshToken == null || 
                _tokenService.IsRefreshTokenExpired(user.RefreshToken) || // expired
                oldAccessToken != user.RefreshToken.LatestJwtAccessToken
                )
                    return BadRequest(new ErrorResponse 
                    { Message = "Provided token is invalid Sign In again",
                        Code = System.Net.HttpStatusCode.BadRequest
                    });

            try
            {
                var isValid = _encryptionService.VerifyHMAC(requestRefToken, user.RefreshToken.EncryptedToken);

                if (!isValid)
                    return Unauthorized(new ErrorResponse
                    {
                        Message = "Invalid Refresh Token",
                        Code = System.Net.HttpStatusCode.Unauthorized
                    });


                string newAccessToken = _tokenService.GenerateJwtToken(user);

                await _appUsersRepository.UpdateUserLatestToken(user, newAccessToken);


                return Ok(new SuccessResponse
                {
                    Message = "Token Refreshed Successfully",
                    Code = System.Net.HttpStatusCode.OK,
                    Data = new AccessTokenResponse
                    {
                        AccessToken = newAccessToken,
                        ValidTo = DateTime.UtcNow
                                    .AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture)
                    }
                });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Token is invalid",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

        }
        
    }
}
