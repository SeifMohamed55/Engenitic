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
        private readonly AppUsersRepository _appUsersRepository;
        private readonly IEncryptionService _encryptionService;
        public TokenController
            (IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            AppUsersRepository appUsersRepository,
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

            string? oldAccessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (oldAccessToken == null)
                return BadRequest("No Token Provided");

            if(_tokenService.IsAccessTokenValid(oldAccessToken))
                return BadRequest("Token is valid");

            var extractedId =  _tokenService.ExtractIdFromExpiredToken(oldAccessToken);

            if (!extractedId.HasValue)
                return BadRequest("Provided token is invalid");

            var user = await _appUsersRepository.GetUserWithTokenAndRoles(extractedId.Value);

            if (user == null || 
                user.RefreshToken == null || 
                user.RefreshToken.ExpiryDate.ToUniversalTime() <= DateTimeOffset.UtcNow
                )
                    return BadRequest("Provided token is invalid Sign In again");


            var dbRefreshToken = user.RefreshToken.EncryptedToken;

            var requestRefToken = HttpContext.Request.Cookies["refreshToken"];
            if(requestRefToken == null)
                return BadRequest("No Refresh Token Provided");

            try
            {
                var isValid = _encryptionService.VerifyHMAC(requestRefToken, dbRefreshToken);

                if (!isValid)
                    return Unauthorized("Invalid user request You have to login!");


                string newAccessToken =  _tokenService.GenerateJwtToken(user);

                return Ok(new AccessTokenResponse
                {
                    AccessToken = newAccessToken,
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes)).ToString("f", CultureInfo.InvariantCulture)
                });
            }
            catch(Exception )
            {
                return BadRequest("Token is invalid");
            }  

        }
        
    }
}
