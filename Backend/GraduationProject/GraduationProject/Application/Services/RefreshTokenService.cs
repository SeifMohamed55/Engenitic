using GraduationProject.Infrastructure.Data;

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
        Task<ServiceResult<string>> Refresh(string oldAccessToken, string requestRefToken);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IJwtTokenService _tokenService;
        private readonly IEncryptionService _encryptionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        public RefreshTokenService
            (IJwtTokenService tokenService,
            IUnitOfWork unitOfWork,
            IEncryptionService encryptionService,
            IUserService userService)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _userService = userService;
        }

        public async Task<ServiceResult<string>> Refresh(string oldAccessToken, string requestRefToken)
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


            var user = await _unitOfWork.UserRepo.GetUserWithTokenAndRoles(extractedId);

            if (
                    user == null ||
                    user.RefreshToken == null ||
                    _tokenService.IsRefreshTokenExpired(user.RefreshToken) || // expired
                    jti != user.RefreshToken.LatestJwtAccessTokenJti
                )
            {
                return ServiceResult<string>.Failure("Provided token is invalid Sign In again");
            }

            try
            {
                var isValid = _tokenService.VerifyRefresh(requestRefToken, user.RefreshToken.EncryptedToken);

                if (!isValid)
                    return ServiceResult<string>.Failure("Invalid Refresh Token");

                (string newAccessToken, string newJti) = _tokenService.GenerateJwtToken(user, user.Roles.Select(x=> x.Name).ToList());

                _userService.UpdateUserLatestToken(user, newJti);
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
