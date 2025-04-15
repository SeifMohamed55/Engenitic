using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;

namespace GraduationProject.Application.Services
{
    public interface ILoginRegisterService
    {
        Task<ServiceResult<LoginWithCookies>> Login(LoginCustomRequest model, DeviceInfo deviceInfo);
        Task<ServiceResult<RefreshToken>> Logout(Guid deviceId, int userId);
        Task<ServiceResult<LoginWithCookies>> ExternalLogin(string provider, AuthenticatedPayload payload);
        Task<ServiceResult<AppUserDTO>> Register(RegisterCustomRequest model, bool isExternal);
    }
    public class LoginRegisterService : ILoginRegisterService
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

        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork,
            ITokenBlacklistService tokenBlacklistService,
            SignInManager<AppUser> signInManager,
            IUploadingService uploadingService,
            ICloudinaryService cloudinaryService,
            IUserService userService
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

        }


        public async Task<ServiceResult<AppUserDTO>> Register(RegisterCustomRequest model, bool isExternal)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return ServiceResult<AppUserDTO>.Failure("Email already exists.");

            if (model.Password != model.ConfirmPassword)
                return ServiceResult<AppUserDTO>.Failure("Passwords do not match.");

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
                    return ServiceResult<AppUserDTO>.Failure("Invalid phone number");
            }

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null)
                return ServiceResult<AppUserDTO>.Failure("Invalid Role.");

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
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                    throw new Exception();
                }

                result = await _userManager.AddToRoleAsync(user, model.Role);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                    throw new Exception();
                }

                await _unitOfWork.CommitTransactionAsync();

            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                var list = errors.Select(x => x.Description).ToList();
                return ServiceResult<AppUserDTO>.Failure(list.Count == 0 ? "An error occured" : string.Join('\n', list));
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
                    Image = new ImageMetadata
                    {
                        ImageURL = imageUrl,
                        Name = imgName,
                        Hash = fileHash.Hash
                    }
                };

                return ServiceResult<AppUserDTO>.Success(data);
            }
            catch
            {
                return ServiceResult<AppUserDTO>.Failure("Couldn't save image");
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
                return ServiceResult<LoginWithCookies>.Failure("Email does not exist");

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return ServiceResult<LoginWithCookies>.Failure("User is LockedOut Try again in 5 minutes");

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                return ServiceResult<LoginWithCookies>.Failure("Password is incorrect!");
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
                    Image = new ImageMetadata
                    {
                        ImageURL = imgUrl,
                        Name = imgName,
                        Hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.Hash ?? 0
                    },
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                    .ToString("f", CultureInfo.InvariantCulture),
                    AccessToken = accessToken
                };

                return ServiceResult<LoginWithCookies>.Success(new LoginWithCookies()
                {
                    LoginResponse = data,
                    RefreshToken = dbToken,
                });
            }
            catch
            {
                return ServiceResult<LoginWithCookies>.Failure("Couldn't SignIn.");
            }

        }


        public async Task<ServiceResult<RefreshToken>> Logout(Guid deviceId, int userId)
        {

            var dbRefreshToken = await _unitOfWork.TokenRepo.GetUserRefreshToken(deviceId, userId);
            if (dbRefreshToken is null)
                return ServiceResult<RefreshToken>.Failure("User is not Signed In");


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

            return ServiceResult<RefreshToken>.Success(dbRefreshToken);

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
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors);
                        throw new Exception();
                    }
                    result = await _userManager.AddToRoleAsync(user, "student");
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors);
                        throw new Exception();
                    }

                    var loginInfo = new UserLoginInfo
                        (provider, payload.UniqueId, provider);

                    result = await _userManager.AddLoginAsync(user, loginInfo);
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors);
                        throw new Exception();
                    }
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

                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return (ServiceResult<LoginWithCookies>.Failure(errors.Select(x => x.Description).ToList()));
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
                    return ServiceResult<LoginWithCookies>.Failure(result.Errors.Select(x => x.Description).ToList());
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
                    Image = new ImageMetadata
                    {
                        ImageURL = imgUrl,
                        Name = imgName,
                        Hash = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.Hash ?? 0
                    },
                    ValidTo = DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                        .ToString("f", CultureInfo.InvariantCulture),
                    AccessToken = accessToken
                };

                return ServiceResult<LoginWithCookies>.Success(new LoginWithCookies()
                {
                    LoginResponse = data,
                    RefreshToken = dbToken,
                });
            }
            catch
            {
                return ServiceResult<LoginWithCookies>.Failure("An Error Occured, Try again later");
            }

            


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
