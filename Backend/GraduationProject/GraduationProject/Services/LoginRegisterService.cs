using Ganss.Xss;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;
using System.Web;
using GraduationProject.Data;

namespace GraduationProject.Services
{
    public interface ILoginRegisterService
    {
        //Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload , HttpContext httpContext);
        Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext);
        Task<IResult> Logout(HttpContext httpContext);

        Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload, HttpContext httpContext);

        Task<IResult> Register(RegisterCustomRequest model);
        Task<IResult> RegisterAdmin(RegisterCustomRequest model);
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
        private readonly ICloudinaryService _cloudinary;
        private readonly IEncryptionService _encryptionService;

        public LoginRegisterService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokenService,
            IOptions<JwtOptions> options,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork,
            ITokenBlacklistService tokenBlacklistService,
            SignInManager<AppUser> signInManager,
            ICloudinaryService cloudinaryService,
            IEncryptionService encryptionService
            )

        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtOptions = options.Value;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _tokenBlacklistService = tokenBlacklistService;
            _signInManager = signInManager;
            _cloudinary = cloudinaryService;
            _encryptionService = encryptionService;
        }


        public async Task<IResult> Register(RegisterCustomRequest model)
        {
            if(await _userManager.FindByEmailAsync(model.Email) != null)
                return Results.BadRequest(new ErrorResponse 
                { 
                    Message = "Email already exists",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            model.Role = model.Role.ToLower();
            if (model.Role != "instructor" && model.Role != "student")
                return Results.BadRequest(new ErrorResponse()
                {
                   Message = "Invalid Role!",
                   Code = System.Net.HttpStatusCode.BadRequest,
                });

            if (model.Password != model.ConfirmPassword)
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message =  "Passwords do not match",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

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
                {
                    return Results.BadRequest(new ErrorResponse() 
                    { 
                        Message = "Invalid phone number",
                        Code = System.Net.HttpStatusCode.BadRequest 
                    });
                }
            }

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null || userRole.NormalizedName == "ADMIN" || userRole.Id == 1)
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Role",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                FullName = model.Username,
            };

            var defaultHash = await _unitOfWork.FileHashRepo
                .FirstOrDefaultAsync(x=> x.PublicId == _cloudinary.DefaultUserImagePublicId);
            
            if(defaultHash == null)
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Default Image not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            string? publicId;

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

                if (ImageHelper.IsValidImageType(model.Image))
                {
                    Debug.Assert(model.Image != null);

                    var imageName = "user_" + user.Id;

                    publicId = await _cloudinary.UploadAsync(model.Image, imageName, CloudinaryType.UserImage);
                    if (publicId == null)
                    {
                        publicId = _cloudinary.DefaultUserImagePublicId;
                        user.FileHashes.Add(defaultHash);
                    }
                    else
                        user.FileHashes.Add(new FileHash()
                        {
                            Type = CloudinaryType.UserImage,
                            PublicId = publicId,
                            Hash = await _encryptionService.HashWithxxHash(model.Image.OpenReadStream())
                        });
                }
                else
                {
                    publicId = _cloudinary.DefaultUserImagePublicId;
                    user.FileHashes.Add(defaultHash);
                }

                await _unitOfWork.SaveChangesAsync();

                var imageUrl = _cloudinary.GetImageUrl(publicId);
                var imgName = imageUrl.Split('/').LastOrDefault() ?? "";

                return Results.Ok(new SuccessResponse()
                {
                    Message = "User created successfully",
                    Data = new AppUserDTO()
                    {
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber ?? "",
                        Id = user.Id,
                        PhoneRegionCode = user.PhoneRegionCode,
                        UserName = user.FullName,
                        Image = new ImageMetadata
                        { 
                            ImageURL = imageUrl, Name = imgName, Hash = user.FileHashes.FirstOrDefault(x=> x.PublicId == publicId)?.Hash ?? 0
                        }
                    },
                    Code = System.Net.HttpStatusCode.OK
                });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "An error Occured please try again later",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            }        
        }

        
        public async Task<IResult> Login(LoginCustomRequest model, HttpContext httpContext)
        {
            var user = await _unitOfWork.UserRepo.GetUserWithTokenAndRoles(model.Email);
            if (user == null)
                return Results.NotFound(new ErrorResponse 
                { 
                    Message = "Email does not exist",
                    Code = System.Net.HttpStatusCode.NotFound
                });

            if(user.RefreshToken != null)
                _tokenBlacklistService.BlacklistToken
                    (user.RefreshToken.LatestJwtAccessTokenJti, user.RefreshToken.LatestJwtAccessTokenExpiry);

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

            if (result.IsLockedOut)
                return Results.BadRequest(new ErrorResponse 
                { 
                    Message = "User is LockedOut Try again in 5 minutes" ,
                    Code = System.Net.HttpStatusCode.BadRequest
                });

            if(!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                return Results.BadRequest(new ErrorResponse
                {
                    Message = "Password is incorrect!",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
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

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
                };

                httpContext.Response.Cookies.Append("refreshToken", raw, cookieOptions);

                var publicId = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.PublicId;

                if (publicId == null)
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);


                string imgUrl = _cloudinary.GetImageUrl(publicId);
                string imgName = publicId.Split('/').LastOrDefault() ?? "default";

                return Results.Ok(new SuccessResponse()
                {
                    Message = "User Logged In successfully",
                    Data = new
                    {
                        user.Id,
                        user.Banned,
                        Name = user.FullName,
                        Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                        ValidTo = DateTime.UtcNow.AddMinutes(
                            double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture),
                        Image = new
                        {
                            Url = imgUrl,
                            Name = imgName
                        },
                        AccessToken = accessToken,

                    },
                    Code = System.Net.HttpStatusCode.OK
                });
            }
            catch
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Couldn't SignIn",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }
            
        }


        public async Task<IResult> Logout(HttpContext httpContext)
        {
            string? refreshToken = httpContext.Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Results.BadRequest(new ErrorResponse() 
                {
                    Message = "RefreshToken is not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                }); 

            string? accessToken = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (accessToken is null)
                return Results.BadRequest(new ErrorResponse() { 
                    Message = "Jwt Token not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            int id; string accessJti;
            try
            {
                (id, accessJti) = _tokenService.ExtractIdAndJtiFromExpiredToken(accessToken); // allow expired token to signout
            }
            catch
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Invalid AccessToken",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }


            var dbRefreshToken = await _unitOfWork.UserRepo.GetUserRefreshToken(id);
            if (dbRefreshToken is null)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "User is not Signed In",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            if (!_tokenService.VerifyRefreshHmac(refreshToken, dbRefreshToken.EncryptedToken))
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Invalid RefreshToken !",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

            if (accessJti != dbRefreshToken.LatestJwtAccessTokenJti)
                return Results.BadRequest(new ErrorResponse() 
                { Message = "Latest AccessToken Doesn't match",
                    Code = System.Net.HttpStatusCode.BadRequest 
                });

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
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "An error occured.",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.Now.AddDays(-1)
            };

            httpContext.Response.Cookies.Append("refreshToken", "", cookieOptions);

            return Results.Ok(new SuccessResponse
            {
                Message = "User Logged Out successfully",
                Code = System.Net.HttpStatusCode.OK
            });

        }


        public async Task<IResult> ExternalLogin(string provider, AuthenticatedPayload payload, HttpContext httpContext)
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

                var defaultHash = await _unitOfWork.FileHashRepo
                    .FirstOrDefaultAsync(x => x.PublicId == _cloudinary.DefaultUserImagePublicId);

                if (defaultHash == null)
                    return Results.BadRequest(new ErrorResponse()
                    {
                        Message = "Default Image not found",
                        Code = System.Net.HttpStatusCode.BadRequest
                    });

                List<string> errors = new List<string>();
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors.Select(x=> x.Description));
                        throw new Exception();
                    }
                    result = await _userManager.AddToRoleAsync(user, "student");
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors.Select(x => x.Description));
                        throw new Exception();
                    }

                    var loginInfo = new UserLoginInfo
                        (provider, payload.UniqueId, provider);

                    result = await _userManager.AddLoginAsync(user, loginInfo);
                    if (!result.Succeeded)
                    {
                        errors.AddRange(result.Errors.Select(x => x.Description));
                        throw new Exception();
                    }
                    await _unitOfWork.CommitTransactionAsync(); // rollsback if something wrong happened

                    var imageName = "user_" + user.Id;

                    string? publicId = await _cloudinary.UploadRemoteAsync(payload.Image, imageName, CloudinaryType.UserImage);
                    if (publicId == null)
                    {
                        publicId = _cloudinary.DefaultUserImagePublicId;
                        user.FileHashes.Add(defaultHash);
                    }
                    else
                    {
                        await using var stream = await _cloudinary.GetFileStreamAsync(publicId);
                        if (stream == null)
                            throw new Exception();

                        user.FileHashes.Add(new FileHash()
                        {
                            Type = CloudinaryType.UserImage,
                            PublicId = publicId,
                            Hash = await _encryptionService.HashWithxxHash(stream)
                        });
                    }
                        

                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Results.BadRequest(new ErrorResponse()
                    {
                        Code = System.Net.HttpStatusCode.BadRequest,
                        Message = errors.Count > 0 ?
                                errors : [ex.Message]
                    });
                }
            }

            var providerLogin = await _unitOfWork.UserLoginRepo.ContainsLoginProvider(user.Id, provider);

            if (!providerLogin)
            {
                var loginInfo = new UserLoginInfo
                       (provider, payload.UniqueId, provider);
                var result = await _userManager.AddLoginAsync(user, loginInfo);
                if (!result.Succeeded)
                    return Results.BadRequest(new ErrorResponse() 
                    { 
                       Message = string.Join(".\n", result.Errors.Select(x => x.Description)),
                       Code = System.Net.HttpStatusCode.BadRequest
                    });
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
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "An Error Occured, Try again later",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }



            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays))
            };

            httpContext.Response.Cookies.Append("refreshToken", raw, cookieOptions);


            string? publicIdimg = user.FileHashes.FirstOrDefault(x => x.Type == CloudinaryType.UserImage)?.PublicId;
            if (publicIdimg == null)
                return Results.StatusCode(StatusCodes.Status500InternalServerError);

            string imgUrl = _cloudinary.GetImageUrl(publicIdimg); 

            var responseObject = new SuccessResponse()
            {
                Message = "User Logged In successfully",
                Data = new
                {
                    user.Id,
                    user.Banned,
                    Name = user.FullName,
                    Roles = user.Roles.Select(x => x.Name.ToLower()).ToList(),
                    ValidTo = DateTime.UtcNow.AddMinutes(
                            double.Parse(_jwtOptions.AccessTokenValidityMinutes))
                                    .ToString("f", CultureInfo.InvariantCulture),
                    Image = new
                    {
                        Url = imgUrl,
                        Name = imgUrl.Split('/').LastOrDefault()
                    },
                    AccessToken = accessToken,

                },

                Code = System.Net.HttpStatusCode.OK
            };

            var jsonResponse = HttpUtility.JavaScriptStringEncode(JsonSerializer.Serialize(responseObject));

            var htmlContent = $@"
                 <html>
                 <head>
                     <title>Authentication Successful</title>
                 </head>
                 <body>
                     <script>
                         (function() {{
                             function sendToken() {{
                                 var data = JSON.parse(""{jsonResponse}""); // ✅ Safely parse JSON
                                 window.opener.postMessage(data, '{_jwtOptions.Audience}');
                                 window.close();
                             }}
                             sendToken();
                         }})();
                     </script>
                     <p>Authentication successful. You can close this window.</p>
                 </body>
                 </html>
            ";

            return Results.Content(htmlContent, "text/html");
        }



        public async Task<IResult> RegisterAdmin(RegisterCustomRequest model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return Results.BadRequest(new ErrorResponse
                {
                    Message = "Email already exists",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


            if (model.Password != model.ConfirmPassword)
            {
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Passwords do not match",
                    Code = System.Net.HttpStatusCode.BadRequest
                });
            }

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
                {
                    return Results.BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid phone number",
                        Code = System.Net.HttpStatusCode.BadRequest
                    });
                }
            }

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                FullName = model.Username,
            };

            var defaultHash = await _unitOfWork.FileHashRepo
                .FirstOrDefaultAsync(x => x.PublicId == _cloudinary.DefaultUserImagePublicId);

            if (defaultHash == null)
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = "Default Image not found",
                    Code = System.Net.HttpStatusCode.BadRequest
                });


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

                string? publicId;

                if (ImageHelper.IsValidImageType(model.Image))
                {
                    Debug.Assert(model.Image != null);

                    var imageName = "user_" + user.Id;

                    publicId = await _cloudinary.UploadAsync(model.Image, imageName, CloudinaryType.UserImage);
                    if (publicId == null)
                    {
                        publicId = _cloudinary.DefaultUserImagePublicId;
                        user.FileHashes.Add(defaultHash);
                    }
                    else
                        user.FileHashes.Add(new FileHash()
                        {
                            Type = CloudinaryType.UserImage,
                            PublicId = publicId,
                            Hash = await _encryptionService.HashWithxxHash(model.Image.OpenReadStream())
                        });
                }
                else
                {
                    publicId = _cloudinary.DefaultUserImagePublicId;
                    user.FileHashes.Add(defaultHash);
                }

                await _unitOfWork.SaveChangesAsync();


                return Results.Ok(new SuccessResponse()
                {
                    Message = "User created successfully",
                    Data = new AppUserDTO()
                    {
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber ?? "",
                        Id = user.Id,
                        PhoneRegionCode = user.PhoneRegionCode,
                        UserName = user.FullName,
                    },
                    Code = System.Net.HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                errors.Add(new IdentityError() { Description = ex.Message });
                return Results.BadRequest(new ErrorResponse()
                {
                    Message = string.Join(".\n", errors.Select(x => x.Description)),
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            }


        }


    }
}
