using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Constants;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sprache;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;

namespace GraduationProject.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly JwtOptions _jwtOptions;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUploadingService _uploadingService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IUserService _userService;
        private readonly IGmailServiceHelper _emailSender;

        public AuthenticationService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork,
            ITokenBlacklistService tokenBlacklistService,
            SignInManager<AppUser> signInManager,
            IUploadingService uploadingService,
            ICloudinaryService cloudinaryService,
            IUserService userService,
            IGmailServiceHelper emailSender
            )

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _tokenBlacklistService = tokenBlacklistService;
            _signInManager = signInManager;
            _uploadingService = uploadingService;
            _cloudinaryService = cloudinaryService;
            _userService = userService;
            _emailSender = emailSender;

        }


        private void HandleIdentityResult(IdentityResult result, List<IdentityError> errors)
        {
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
                throw new Exception();
            }
        }

        public async Task<ServiceResult<AppUserDTO>> Register(RegisterCustomRequest model, bool isExternal)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return ServiceResult<AppUserDTO>.Failure("Email already exists.", HttpStatusCode.BadRequest);

            if (model.Password != model.ConfirmPassword)
                return ServiceResult<AppUserDTO>.Failure("Passwords do not match.", HttpStatusCode.BadRequest);

            string? RegionCode = null;
            if (model.PhoneNumber != null && model.PhoneRegion != null)
            {
                (string, string)? phoneDetails = PhoneNumberService
                                        .IsValidPhoneNumber(model.PhoneNumber, model.PhoneRegion);
                if (phoneDetails.HasValue)
                {
                    model.PhoneNumber = phoneDetails.Value.Item1;
                    RegionCode = phoneDetails.Value.Item2;
                }
                else
                    return ServiceResult<AppUserDTO>.Failure("Invalid phone number", HttpStatusCode.BadRequest);
            }
            if (model.Role == Roles.Instructor)
                model.Role = Roles.UnverifiedInstructor;

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null)
                return ServiceResult<AppUserDTO>.Failure("Invalid Role.", HttpStatusCode.BadRequest);

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                FullName = model.Username,
                IsExternal = isExternal
            };


            List<IdentityError> errors = new List<IdentityError>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var result = await _userManager.CreateAsync(user, model.Password);
                HandleIdentityResult(result, errors);

                result = await _userManager.AddToRoleAsync(user, model.Role);
                HandleIdentityResult(result, errors);

                await _unitOfWork.CommitTransactionAsync();

            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();               
                return ServiceResult<AppUserDTO>.Failure("An error occured", errors);
            }

            try
            {
                var fileHash = await _unitOfWork.FileHashRepo.GetDefaultUserImageAsync();

                if (UploadingService.IsValidImageType(model.Image))
                {
                    var imageName = $"user_{user.Id}";

                    using (Stream stream = model.Image.OpenReadStream())
                    {
                        fileHash = await TryUploadImage(stream, imageName, fileHash);
                    }
                }

                user.FileHashes.Add(fileHash);


                if (userRole.Name.Equals(Roles.UnverifiedInstructor, StringComparison.OrdinalIgnoreCase))
                {
                    if (!UploadingService.IsValidCv(model.Cv))
                    {
                        await _userManager.DeleteAsync(user);
                        return ServiceResult<AppUserDTO>.Failure("Invalid CV file type.", HttpStatusCode.BadRequest);
                    }

                    using (Stream cvStream = model.Cv.OpenReadStream())
                    {
                        var cvHash = await _uploadingService.UploadImageAsync(cvStream, $"cv_{user.Id}", CloudinaryType.InstructorCV);
                        if (cvHash != null)
                        {
                            user.FileHashes.Add(cvHash);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var imageUrl = _cloudinaryService.GetImageUrl(fileHash.PublicId, fileHash.Version);
                var imgName = imageUrl.Split('/').LastOrDefault() ?? "";

                var data = new AppUserDTO
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Id = user.Id,
                    PhoneRegionCode = user.PhoneRegionCode,
                    UserName = user.FullName,
                    Image = new FileMetadata
                    {
                        ImageURL = imageUrl,
                        Name = imgName,
                        Hash = fileHash.Hash
                    }
                };

                return ServiceResult<AppUserDTO>.Success(data, "User registered Successfully.", HttpStatusCode.OK);
            }
            catch
            {
                return ServiceResult<AppUserDTO>.Failure("Couldn't save image", HttpStatusCode.BadRequest);
            }
        }

        private async Task<FileHash> TryUploadImage(Stream stream, string imageName, FileHash defaultHash)
        {
            try
            {
                var fileHash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.UserImage);

                if (fileHash != null)
                {
                    _unitOfWork.FileHashRepo.Insert(fileHash);
                    await _unitOfWork.SaveChangesAsync();
                }        

                return fileHash ?? defaultHash;

            }
            catch 
            {
                return defaultHash;
            }
        }


        public async Task<ServiceResult<LoginWithCookies>> Login(LoginCustomRequest model, DeviceInfo deviceInfo)
        {
            var user = await _unitOfWork.UserRepo.GetUserWithRoles(model.Email);
            if (user == null)
                return ServiceResult<LoginWithCookies>.Failure("Email does not exist", HttpStatusCode.BadRequest);

            if(user.Banned)
                return ServiceResult<LoginWithCookies>.Failure("User is banned", HttpStatusCode.Forbidden);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return ServiceResult<LoginWithCookies>.Failure("User is LockedOut Try again in 5 minutes", HttpStatusCode.Locked);

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                return ServiceResult<LoginWithCookies>.Failure("Password is incorrect!", HttpStatusCode.BadRequest);
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            try
            {
                await _unitOfWork.TokenRepo.RemoveRevokedOrExpiredByUserId(user.Id);

                var dbToken = await _unitOfWork.TokenRepo.GetUserRefreshToken(deviceInfo.DeviceId, user.Id);

                if (dbToken == null)
                    dbToken = _unitOfWork.TokenRepo.GenerateRefreshToken(user.Id, deviceInfo);

                (string accessToken, string jti) = _tokenService.GenerateJwtToken(user, user.Roles.Select(x => x.Name).ToList());

                dbToken.LatestJwtAccessTokenJti = jti;
                dbToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                    (double.Parse(_jwtOptions.AccessTokenValidityMinutes));
                dbToken.RememberMe = model.RememberMe;

                await _unitOfWork.SaveChangesAsync();

                var hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage);
                string imgUrl = "";
                string imgName = "";

                if (hash == null)
                {
                    imgUrl = _cloudinaryService.GetImageUrl(ICloudinaryService.DefaultUserImagePublicId, "1");
                    imgName = "default"; 

                }
                else
                {
                     imgUrl = _cloudinaryService.GetImageUrl(hash.PublicId, hash.Version);
                     imgName = hash.PublicId.Split('/').LastOrDefault() ?? "default";
                }

                var data = new LoginResponse
                {
                    Id = user.Id,
                    Banned = user.Banned,
                    Name = user.FullName,
                    Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                    Image = new FileMetadata
                    {
                        ImageURL = imgUrl,
                        Name = imgName,
                        Hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.Hash ?? 0
                    },
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                    .ToString("f", CultureInfo.InvariantCulture),
                    AccessToken = accessToken
                };

                return ServiceResult<LoginWithCookies>.Success(
                    new LoginWithCookies()
                    {
                        LoginResponse = data,
                        RefreshToken = dbToken,
                    },
                    "Logged in successfully",
                    HttpStatusCode.OK
                );
            }
            catch
            {
                return ServiceResult<LoginWithCookies>.Failure("Couldn't SignIn.", HttpStatusCode.BadRequest);
            }

        }


        public async Task<ServiceResult<RefreshToken>> Logout(Guid deviceId, int userId)
        {
            
            var dbRefreshToken = await _unitOfWork.TokenRepo.GetUserRefreshToken(deviceId, userId);
            if (dbRefreshToken is null)
                return ServiceResult<RefreshToken>.Failure("User is not Signed In", HttpStatusCode.OK);


            _tokenBlacklistService.BlacklistToken
                (dbRefreshToken.LatestJwtAccessTokenJti, dbRefreshToken.LatestJwtAccessTokenExpiry);

            try
            {
                dbRefreshToken.IsRevoked = true;
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
            }
            return ServiceResult<RefreshToken>.Success(dbRefreshToken, "Logged out successfully", HttpStatusCode.OK);

        }


        public async Task<ServiceResult<LoginWithCookies>> ExternalLogin(string provider, AuthenticatedPayload payload)
        {
            var user = await _unitOfWork.UserRepo.GetUserWithRoles(payload.Email);

            if (user == null)
            {

                user = new AppUser()
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    Banned = false,
                    FullName = payload.Name,
                    IsExternal = true,
                };

                List<IdentityError> errors = new List<IdentityError>();
                try
                {
                    var defaultHash = await _unitOfWork.FileHashRepo.GetDefaultUserImageAsync();

                    await _unitOfWork.BeginTransactionAsync();

                    var result = await _userManager.CreateAsync(user);
                    HandleIdentityResult(result, errors);

                    result = await _userManager.AddToRoleAsync(user, "student");
                    HandleIdentityResult(result, errors);


                    var loginInfo = new UserLoginInfo
                        (provider, payload.UniqueId, provider);

                    result = await _userManager.AddLoginAsync(user, loginInfo);
                    HandleIdentityResult(result, errors);

                    await _unitOfWork.CommitTransactionAsync();

                    var imageName = "user_" + user.Id;

                    FileHash? fileHash;

                    if (payload.ImageUrl != ICloudinaryService.DefaultUserImagePublicId)
                        fileHash = await _uploadingService.UploadImageAsync
                                                (payload.ImageUrl, imageName, CloudinaryType.UserImage);
                    else
                        fileHash = defaultHash;

                    if (fileHash == null)
                        fileHash = defaultHash;

                    user.FileHashes.Add(fileHash);

                    await _unitOfWork.SaveChangesAsync();

                    var imageUrl = _cloudinaryService.GetImageUrl(fileHash.PublicId, fileHash.Version);

                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceResult<LoginWithCookies>.Failure("Failed To login", errors);
                }
            }
            List<string> roles = new List<string>();
            try
            {
                roles = (await _userManager.GetRolesAsync(user)).ToList();
                //await SaveGooglePhoto(user, payload);
            }
            catch
            {
            }
 
            var providerLogin = await _unitOfWork.UserLoginRepo.ContainsLoginProvider(user.Id, provider);

            if (!providerLogin)
            {
                var loginInfo = new UserLoginInfo
                       (provider, payload.UniqueId, provider);

                var result = await _userManager.AddLoginAsync(user, loginInfo);
                if (!result.Succeeded)
                    return ServiceResult<LoginWithCookies>.Failure("Failed to login", result.Errors);
            }

            try
            {
                await _unitOfWork.TokenRepo.RemoveRevokedOrExpiredByUserId(user.Id);

                var dbToken = await _unitOfWork.TokenRepo.GetUserRefreshToken(payload.DeviceInfo.DeviceId, user.Id);

                if (dbToken == null)
                    dbToken = _unitOfWork.TokenRepo.GenerateRefreshToken(user.Id, payload.DeviceInfo);
                
                (string accessToken, string jti) = _tokenService.GenerateJwtToken(user, user.Roles.Select(x => x.Name).ToList());

                dbToken.LatestJwtAccessTokenJti = jti;
                dbToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                    (double.Parse(_jwtOptions.AccessTokenValidityMinutes));

                await _unitOfWork.SaveChangesAsync();

                var hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage);
                string imgUrl = "";
                string imgName = "";

                if (hash == null)
                {
                    imgUrl = _cloudinaryService.GetImageUrl(ICloudinaryService.DefaultUserImagePublicId, "1");
                    imgName = "default";

                }
                else
                {
                    imgUrl = _cloudinaryService.GetImageUrl(hash.PublicId, hash.Version);
                    imgName = hash.PublicId.Split('/').LastOrDefault() ?? "default";
                }

                var data = new LoginResponse
                {
                    Id = user.Id,
                    Banned = user.Banned,
                    Name = user.FullName,
                    Roles = roles.ToList(),
                    Image = new FileMetadata
                    {
                        ImageURL = imgUrl,
                        Name = imgName,
                        Hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.Hash ?? 0
                    },
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                        .ToString("f", CultureInfo.InvariantCulture),
                    AccessToken = accessToken
                };

                return ServiceResult<LoginWithCookies>.Success(
                    new LoginWithCookies()
                    {
                        LoginResponse = data,
                        RefreshToken = dbToken,
                    },
                    "User Logged in successfully"
                    ,HttpStatusCode.OK
                );
            }
            catch
            {
                return ServiceResult<LoginWithCookies>.Failure("An Error Occured, Try again later", HttpStatusCode.BadRequest);
            }
        }

        public async Task<ServiceResult<bool>> ForgetPassword(ForgetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found", HttpStatusCode.BadRequest);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{_jwtOptions.Audience}/reset-password?token={token}&email={model.Email}";
            try
            {
                await _emailSender.SendEmailAsync(model.Email,
                    "Reset Password",
                    $"You can reset your password by clicking this link: {callbackUrl}");

                return ServiceResult<bool>.Success(true, "An email was sent for your new email", HttpStatusCode.OK);
            }
            catch
            {
                return ServiceResult<bool>.Failure("Couldn't send email", HttpStatusCode.BadRequest);
            }
        }

        public async Task<ServiceResult<bool>> ResetPassword(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return ServiceResult<bool>.Failure("User not found", HttpStatusCode.BadRequest);

            if(model.NewPassword != model.ConfirmPassword)
                return ServiceResult<bool>.Failure("Passwords do not match", HttpStatusCode.BadRequest);

            if(await _userManager.CheckPasswordAsync(user, model.NewPassword))
                return ServiceResult<bool>.Failure("New password cannot be the same as old password", HttpStatusCode.BadRequest);

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                return ServiceResult<bool>.Failure("An error occured.", result.Errors);
            }
            return ServiceResult<bool>.Success(true, "Password resetted successfully", HttpStatusCode.OK);
        }

        private async Task SaveGooglePhoto(AppUser user, AuthenticatedPayload payload)
        {
            var dbFileHash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage) ??
            throw new ArgumentNullException("invalid state");

            if (!await _uploadingService.ImageHashMatches(dbFileHash, payload.ImageUrl))
            {
                var imageName = "user_" + user.Id;

                FileHash? fileHash = await _uploadingService.UploadImageAsync(payload.ImageUrl, imageName, CloudinaryType.UserImage);

                if (fileHash != null)
                {
                    if (dbFileHash.PublicId == ICloudinaryService.DefaultUserImagePublicId)
                    {
                        user.FileHashes.Remove(dbFileHash);
                        user.FileHashes.Add(fileHash);
                    }
                    else
                        dbFileHash.UpdateFromHash(fileHash);

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

    }
}
