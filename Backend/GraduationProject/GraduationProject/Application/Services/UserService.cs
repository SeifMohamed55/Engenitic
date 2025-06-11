using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GraduationProject.Application.Services
{

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
            (IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IGmailServiceHelper emailService,
            IOptions<JwtOptions> options,
            ICloudinaryService cloudinary,
            IUploadingService uploadingService,
            IHashingService hashingService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _jwtOptions = options.Value;
            _cloudinary = cloudinary;
            _uploadingService = uploadingService;
            _hashingService = hashingService;
        }

        public async Task<ServiceResult<AppUserDTO>> GetProfile(int userId)
        {

            var userDto = await _unitOfWork.UserRepo.GetAppUserDTO(userId);
            if (userDto == null)
            {
                return ServiceResult<AppUserDTO>.Failure("User not found");
            }

            var imageUrl = _cloudinary.GetImageUrl(userDto.Image.ImageUrl, userDto.Image.Version);
            var imageName = imageUrl.Split('/').LastOrDefault() ?? "";
            userDto.Image.ImageUrl = imageUrl;
            userDto.Image.Name = imageName;

            return ServiceResult<AppUserDTO>.Success(userDto, "Profile retrieved Successfully.");
        }


        public async Task<ServiceResult<bool>> UpdateEmail(UpdateEmailRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.Id.ToString());
            if (user == null)
                return ServiceResult<bool>.Failure("User not found");

            var emailExists = await _userManager.FindByEmailAsync(req.NewEmail) != null;
            if (emailExists)
                return ServiceResult<bool>.Failure("Email Already exist");


            var token = await _userManager.GenerateChangeEmailTokenAsync(user, req.NewEmail);

            var confirmationLink = $"{_jwtOptions.Audience}/redirection-page?userId={user.Id}&newEmail={req.NewEmail}&token={Uri.EscapeDataString(token)}";

            // Send this link to the user's email
            await _emailService.SendEmailAsync(req.NewEmail,
                "Confirm Email Change",
                $"Please confirm your email change by clicking this link: {confirmationLink}");

            await _emailService.SendEmailAsync(user.Email, "Email Change Requested",
                                $"If you did not request this change to {req.NewEmail}, please contact support immediately.");

            return ServiceResult<bool>.Success(true, "Email was sent successfully");

        }

        public async Task<ServiceResult<bool>>ConfirmEmailChange(ConfirmEmailRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId.ToString());
            if (user == null)
            {
               return ServiceResult<bool>.Failure("User not found");
            }

            var emailExists = await _userManager.FindByEmailAsync(req.NewEmail) != null;

            if (emailExists)
                return ServiceResult<bool>.Failure("Email Already exist");

            var result = await _userManager.ChangeEmailAsync(user, req.NewEmail, req.Token);
            if (result.Succeeded)
            {
                return ServiceResult<bool>.Success(true, "Email Verified Successfully");
            }
            else
            {
                return ServiceResult<bool>.Failure("Failed to change email");
            }
        }

        public async Task<ServiceResult<bool>> UpdatePassword(UpdatePasswordRequest req, string claimId)
        {
            var user = await _userManager.FindByIdAsync(claimId);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found");

            var res = await _userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);
            if (!res.Succeeded)
                return ServiceResult<bool>.Failure(string.Join('\n', res.Errors.Select(x=> x.Description)));

            return ServiceResult<bool>.Success(true, "Password updated successfully");  
        }

        public async Task<ServiceResult<bool>> UpdateUsernameRequest(UpdateUsernameRequest req, string claimId)
        {
            var user = await _userManager.FindByIdAsync(claimId);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found");

            if (user.FullName == req.NewUsername)
                return ServiceResult<bool>.Failure("New username is the same as the current one");

            user.FullName = req.NewUsername;
            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                return ServiceResult<bool>.Failure(string.Join('\n', res.Errors.Select(x => x.Description)));

            return ServiceResult<bool>.Success(true, "Username updated successfully");
        }
    }
}
