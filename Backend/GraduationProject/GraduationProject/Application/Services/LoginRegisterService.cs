using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;

namespace GraduationProject.Application.Services
{
    public interface ILoginRegisterService
    {
        Task<(ServiceResult<LoginResponse>, string?)> Login(LoginCustomRequest model);
        Task<ServiceResult<string>> Logout(string accessToken, string refreshToken);
        Task<(ServiceResult<LoginResponse>, string?)> ExternalLogin(string provider, AuthenticatedPayload payload);
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

        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork,
            ITokenBlacklistService tokenBlacklistService,
            SignInManager<AppUser> signInManager,
            IUploadingService uploadingService,
            ICloudinaryService cloudinaryService
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

                FileHash? fileHash;
                var defaultHash = await _unitOfWork.FileHashRepo.GetDefaultUserImageAsync();

                if (UploadingService.IsValidImageType(model.Image))
                {
                    var imageName = "user_" + user.Id;

                    using var stream = model.Image.OpenReadStream();

                    fileHash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.UserImage);

                    if (fileHash == null)
                        fileHash = defaultHash;
                }
                else
                {
                    fileHash = defaultHash;
                }
                    
                user.FileHashes.Add(fileHash);

                await _unitOfWork.SaveChangesAsync();

                var imageUrl = _cloudinaryService.GetImageUrl(fileHash.PublicId);
                var imgName = imageUrl.Split('/').LastOrDefault() ?? "";

                var data = new AppUserDTO()
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
                await _unitOfWork.RollbackTransactionAsync();
                return ServiceResult<AppUserDTO>.Failure(errors.Select(x => x.Description).ToList());
            }
        }


        public async Task<(ServiceResult<LoginResponse>, string?)> Login(LoginCustomRequest model)
        {
            var user = await _unitOfWork.UserRepo.GetUserWithTokenAndRoles(model.Email);
            if (user == null)
                return (ServiceResult<LoginResponse>.Failure("Email does not exist"), null);


            if (user.RefreshToken != null)
                _tokenBlacklistService.BlacklistToken
                    (user.RefreshToken.LatestJwtAccessTokenJti, user.RefreshToken.LatestJwtAccessTokenExpiry);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return (ServiceResult<LoginResponse>.Failure("User is LockedOut Try again in 5 minutes"), null);

            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                return (ServiceResult<LoginResponse>.Failure("Password is incorrect!"), null);
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

            (string accessToken, string jti) = _tokenService.GenerateJwtToken(user);

            refreshToken.LatestJwtAccessTokenJti = jti;
            refreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                (double.Parse(_jwtOptions.AccessTokenValidityMinutes));
            try
            {
                _unitOfWork.UserRepo.UpdateRefreshToken(user, refreshToken);
                await _unitOfWork.SaveChangesAsync();



                var publicId = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.PublicId;

                if (publicId == null)
                    publicId = ICloudinaryService.DefaultUserImagePublicId;


                string imgUrl = _cloudinaryService.GetImageUrl(publicId);
                string imgName = publicId.Split('/').LastOrDefault() ?? "default";

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

                return (ServiceResult<LoginResponse>.Success(data), raw);

            }
            catch
            {
                return (ServiceResult<LoginResponse>.Failure("Couldn't SignIn."), null);
            }

        }


        public async Task<ServiceResult<string>> Logout(string accessToken, string refreshToken)
        {
            int id; string accessJti;
            try
            {
                (id, accessJti) = _tokenService.ExtractIdAndJtiFromExpiredToken(accessToken); // allow expired token to signout
            }
            catch
            {
                return ServiceResult<string>.Failure("Invalid access token.");
            }


            var dbRefreshToken = await _unitOfWork.UserRepo.GetUserRefreshToken(id);
            if (dbRefreshToken is null)
                return ServiceResult<string>.Failure("User is not Signed In");

            if (!_tokenService.VerifyRefreshHmac(refreshToken, dbRefreshToken.EncryptedToken))
                return ServiceResult<string>.Failure("Invalid refresh token.");


            if (accessJti != dbRefreshToken.LatestJwtAccessTokenJti)
                return ServiceResult<string>.Failure("Latest AccessToken Doesn't match.");

            // Get Latest Access Token (from database) and Blacklist it if it's the one sent else ignore it
            _tokenBlacklistService.BlacklistToken
                (dbRefreshToken.LatestJwtAccessTokenJti, dbRefreshToken.LatestJwtAccessTokenExpiry);

            try
            {
                _unitOfWork.TokenRepo.DeleteRefreshToken(dbRefreshToken.Id);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                return ServiceResult<string>.Failure("An error occured.");
            }

            return ServiceResult<string>.Success("User Logged Out successfully");

        }


        public async Task<(ServiceResult<LoginResponse>, string?)> ExternalLogin(string provider, AuthenticatedPayload payload)
        {
            var user = await _userManager.FindByEmailAsync(payload.Email);

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
                    FileHash? fileHash = await _uploadingService.UploadImageAsync
                                            (payload.ImageUrl, imageName, CloudinaryType.UserImage);

                    if (fileHash == null)
                        fileHash = defaultHash;

                    user.FileHashes.Add(fileHash);

                    await _unitOfWork.SaveChangesAsync();

                    var imageUrl = _cloudinaryService.GetImageUrl(fileHash.PublicId);

                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return (ServiceResult<LoginResponse>.Failure(errors.Select(x => x.Description).ToList()), null);
                }
            }

            var providerLogin = await _unitOfWork.UserLoginRepo.ContainsLoginProvider(user.Id, provider);

            if (!providerLogin)
            {
                var loginInfo = new UserLoginInfo
                       (provider, payload.UniqueId, provider);

                var result = await _userManager.AddLoginAsync(user, loginInfo);
                if (!result.Succeeded)
                    return (ServiceResult<LoginResponse>.Failure(result.Errors.Select(x => x.Description).ToList()), null);
            }

            (RefreshToken refreshToken, string raw) = _tokenService.GenerateRefreshToken(user);

            (string accessToken, string jti) = _tokenService.GenerateJwtToken(user);

            refreshToken.LatestJwtAccessTokenJti = jti;
            refreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                (double.Parse(_jwtOptions.AccessTokenValidityMinutes));

            try
            {
                _unitOfWork.UserRepo.UpdateRefreshToken(user, refreshToken);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                return (ServiceResult<LoginResponse>.Failure("An Error Occured, Try again later"), null);
            }





            string? publicIdimg = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.PublicId;
            if (publicIdimg == null)
                publicIdimg = ICloudinaryService.DefaultUserImagePublicId;

            string imgUrl = _cloudinaryService.GetImageUrl(publicIdimg);
            string imgName = publicIdimg.Split('/').LastOrDefault() ?? "default";

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

            return (ServiceResult<LoginResponse>.Success(data), raw);


        }

    }
}
