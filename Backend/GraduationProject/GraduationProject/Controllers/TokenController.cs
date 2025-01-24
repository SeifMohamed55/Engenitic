using GraduationProject.Controllers.APIResponses;
using GraduationProject.Repositories;
using GraduationProject.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly AppUsersRepository _appUsersRepository;
        private readonly IAesEncryptionService _aesEncryptionService;
        public TokenController
            (IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            AppUsersRepository appUsersRepository,
            IAesEncryptionService aesEncryptionService)
        {
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _appUsersRepository = appUsersRepository;
            _aesEncryptionService = aesEncryptionService;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {

            string? oldAccessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (oldAccessToken == null)
                return BadRequest("No Token Provided");

            if(_tokenService.IsTokenValid(oldAccessToken))
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

            var encryptedCookieRefreshToken = HttpContext.Request.Cookies["refreshToken"];
            if(encryptedCookieRefreshToken == null)
                return BadRequest("No Token Provided");

            try
            {
                var oldRefreshToken = _aesEncryptionService.Decrypt(dbRefreshToken);
                var cookieRefreshToken = _aesEncryptionService.Decrypt(encryptedCookieRefreshToken);

                if (oldRefreshToken == null || cookieRefreshToken == null || oldRefreshToken != cookieRefreshToken)
                    return Unauthorized("Invalid user request You have to login!");


                string newAccessToken =  _tokenService.GenerateJwtToken(user);


                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                };
                HttpContext.Response.Cookies.Append("refreshToken", user.RefreshToken.EncryptedToken, cookieOptions);

                return Ok(new AccessTokenResponse
                {
                    AccessToken = newAccessToken,
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes)).ToString("f")
                });
            }
            catch(Exception )
            {
                return BadRequest("Token is invalid");
            }  

        }
        
    }
}
