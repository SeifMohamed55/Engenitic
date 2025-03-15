using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GraduationProject.Application.Services
{

    public interface IUserService
    {
        void UpdateUserLatestToken(AppUser user, string latestToken);
        void UpdateRefreshToken(AppUser appUser, RefreshToken token);

    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGmailServiceHelper _emailService;
        private readonly JwtOptions _jwtOptions;
        private readonly ICloudinaryService _cloudinary;
        private readonly IUploadingService _uploadingService;
        private readonly IHashingService _hashingService;

        public UserService
            (
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IGmailServiceHelper emailService,
            IOptions<JwtOptions> options,
            ICloudinaryService cloudinary,
            IUploadingService uploadingService,
            IHashingService hashingService
            )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _jwtOptions = options.Value;
            _cloudinary = cloudinary;
            _uploadingService = uploadingService;
            _hashingService = hashingService;
        }

        public void UpdateRefreshToken(AppUser appUser, RefreshToken token)
        {
            if(appUser.RefreshToken != null)
                _unitOfWork.TokenRepo.Detach(appUser.RefreshToken);
            appUser.RefreshToken = token;
        }

        public void UpdateUserLatestToken(AppUser user, string latestJti)
        {
            if (user.RefreshToken == null)
                throw new ArgumentNullException();

            user.RefreshToken.LatestJwtAccessTokenJti = latestJti;
            user.RefreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                                                        (double.Parse(_jwtOptions.AccessTokenValidityMinutes));
        }


    }
}
