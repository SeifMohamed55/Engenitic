using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.Extensions.Options;

namespace GraduationProject.Application.Services
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Refreshes the access token
        /// </summary>
        /// <param name="oldAccessToken"></param>
        /// <returns>
        /// The new Access Token
        /// </returns>
        Task<ServiceResult<string>> Refresh(string oldAccessToken, string requestRefToken, Guid deviceId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IJwtTokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtOptions _jwtOptions;
        public RefreshTokenService
            (IJwtTokenService tokenService,
            IUnitOfWork unitOfWork,
            IUserService userService,
            IOptions<JwtOptions> jwtOptions)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<ServiceResult<string>> Refresh(string oldAccessToken, string requestRefToken, Guid deviceId)
        {

            if (_tokenService.IsAccessTokenValid(oldAccessToken))
                return ServiceResult<string>.Failure("Access token is still valid.");

            int extractedId; string jti;
            try
            {
                (extractedId, jti) = _tokenService.ExtractIdAndJtiFromExpiredToken(oldAccessToken);
            }
            catch (Exception)
            {
                return ServiceResult<string>.Failure("Provided token is invalid");
            }


            var refreshToken = await _unitOfWork.TokenRepo.GetUserRefreshToken(deviceId);
            if(refreshToken == null)
                return ServiceResult<string>.Failure("Provided token is invalid Sign In again");

            if (refreshToken.UserId != extractedId || refreshToken.Token.ToString() != requestRefToken)
                return ServiceResult<string>.Failure("Invalid User request.");

            if (_tokenService.IsRefreshTokenExpired(refreshToken) || jti != refreshToken.LatestJwtAccessTokenJti)            
                return ServiceResult<string>.Failure("Session Expired.");
            

            try
            {
                var user = await _unitOfWork.UserRepo.GetUserWithRoles(extractedId);
                if(user == null)
                    return ServiceResult<string>.Failure("User not found.");

                (string newAccessToken, string newJti) = _tokenService.GenerateJwtToken(user, user.Roles.Select(x => x.Name).ToList());

                refreshToken.LatestJwtAccessTokenJti = newJti;
                refreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                    (double.Parse(_jwtOptions.AccessTokenValidityMinutes));

                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<string>.Success(newAccessToken);

            }
            catch (Exception)
            {
                return ServiceResult<string>.Failure("An error occured.");
            }


        }
    }
}
