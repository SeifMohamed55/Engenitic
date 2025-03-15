using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using GraduationProject.API.Requests;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGmailServiceHelper _emailService;
        private readonly JwtOptions _jwtOptions;
        private readonly ICloudinaryService _cloudinary;
        private readonly IUploadingService _uploadingService;
        private readonly IHashingService _hashingService;

        public UsersController(
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

        // GET: api/Users/
        [HttpGet("profile")]
        public async Task<ActionResult<AppUserDTO>> GetProfileData([FromQuery] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId) || currentUserId != id)
            {
                return Unauthorized(new ErrorResponse()
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Invalid User ID."
                });
            }

            var appUser = await _unitOfWork.UserRepo.GetAppUserDTO(currentUserId);

            if (appUser == null)
                return NotFound(new ErrorResponse()
                {
                    Code = HttpStatusCode.NotFound,
                    Message = "User not found."
                });


            var imageUrl = _cloudinary.GetImageUrl(appUser.Image.ImageURL);
            var imageName = imageUrl.Split('/').LastOrDefault() ?? "";
            appUser.Image.ImageURL = imageUrl;
            appUser.Image.Name = imageName;


            return Ok(new SuccessResponse()
            {
                Data = appUser,
                Message = "User fetched successfully!"
            });
        }

       
        // Update user email
        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest emailRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId) || currentUserId != emailRequest.Id)
            {
                return Unauthorized(new ErrorResponse()
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Invalid User ID."
                });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "User does not exist"
                    });

                var emailExists = await _userManager.FindByEmailAsync(emailRequest.NewEmail) != null;
                if (emailExists)
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "Email Already exist"
                    });

                var token = await _userManager.GenerateChangeEmailTokenAsync(user, emailRequest.NewEmail);

                var confirmationLink = $"{_jwtOptions.Audience}/confirm-email-change?userId={user.Id}&newEmail={emailRequest.NewEmail}&token={Uri.EscapeDataString(token)}";


                // Send this link to the user's email
                await _emailService.SendEmailAsync(emailRequest.NewEmail,
                    "Confirm Email Change",
                    $"Please confirm your email change by clicking this link: {confirmationLink}");

                await _emailService.SendEmailAsync(user.Email, "Email Change Requested",
                                    $"If you did not request this change, please contact support immediately.");

                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Message = "Confirmation email sent."
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Something wrong happended"
                });
            }

        }


        [HttpPost("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange(ConfirmEmailRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null)
            {
                return BadRequest(new ErrorResponse(){
                    Message = "User not found.",
                    Code = HttpStatusCode.BadRequest
                });
            }

            var emailExists = await _userManager.FindByEmailAsync(req.NewEmail) != null;

            if (emailExists)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Email Already exists"
                });

            var result = await _userManager.ChangeEmailAsync(user, req.NewEmail, req.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new SuccessResponse()
            {
                Code = HttpStatusCode.OK,
                Message = "Email verified successfully",
                Data = req.NewEmail
            });

        }


        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = ModelState
                });

            if (req.OldPassword == req.NewPassword)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Passwords cannot be the same"
                });

            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null || req.Id.ToString() != claimId)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "invalid request ids don't match "
                });


            var user = await _userManager.FindByIdAsync(claimId);
            if (user == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "User not found"
                });


            var res = await _userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);
            if (!res.Succeeded)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = res.Errors
                });
            return Ok(new SuccessResponse()
            {
                Code = HttpStatusCode.OK,
                Message = "Password changed Successfully."
            });
        }


        [HttpPost("update-username")]
        public async Task<IActionResult> UpdateUsername(UpdateUsernameRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = ModelState
                });

            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null || req.Id.ToString() != claimId)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "invalid request ids don't match "
                });

            var user = await _userManager.FindByIdAsync(claimId);
            if (user == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "User does not exist."
                });

            if (user.FullName == req.NewUsername)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Username cannot be the same."
                });

            user.FullName = req.NewUsername;
            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = res.Errors
                });

            return Ok(new SuccessResponse()
            {
                Code = HttpStatusCode.OK,
                Message = "Username updated successfully."
            });

        }


        [HttpPost("update-image")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile image, [FromForm] int id)
        {
            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null || claimId != id.ToString())
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Ids don't match"
                });

            if (!UploadingService.IsValidImageType(image))
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid Image"
                });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(claimId);

                if(user == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "User not found."
                    });

                using var imgStream = image.OpenReadStream();

                var dbFileHash = await _unitOfWork.UserRepo.GetUserImageHash(id);

                var imgHash = await _hashingService.HashWithxxHash(imgStream);

                if (dbFileHash.Hash == imgHash)
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "This image is the same as the old one."
                    });
                }
                
                imgStream.Position = 0;

                var imageName = "user_" + user.Id;

                FileHash? fileHash = await _uploadingService.UploadImageAsync(imgStream, imageName, CloudinaryType.UserImage);

                if (fileHash == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "Invalid Image, Failed to upload."
                    });

                if (dbFileHash == null)
                {
                    dbFileHash = new FileHash()
                    {
                        PublicId = fileHash.PublicId,
                        Type = CloudinaryType.UserImage,
                        Hash = fileHash.Hash,
                    };
                    user.FileHashes.Add(dbFileHash);
                }
                else
                {
                    dbFileHash.Hash = fileHash.Hash;
                    dbFileHash.Type = CloudinaryType.UserImage;
                    dbFileHash.PublicId = fileHash.PublicId;
                }
                await _unitOfWork.SaveChangesAsync();


                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Message = "Image Updated Successfully"
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Something wrong happend."
                });
            }


        }




    }
}
