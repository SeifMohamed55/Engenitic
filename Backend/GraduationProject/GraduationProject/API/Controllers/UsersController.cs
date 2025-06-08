using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using GraduationProject.API.Requests;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Infrastructure.Data.Interfaces;
using Humanizer;
using Google.Apis.Util;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Common.Extensions;

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
        private readonly IUserService _userService;

        public UsersController(
            IUserService userService,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IGmailServiceHelper emailService,
            IOptions<JwtOptions> options,
            ICloudinaryService cloudinary,
            IUploadingService uploadingService,
            IHashingService hashingService
            )
        {
            _userService = userService;
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
        public async Task<ActionResult<AppUserDTO>> GetProfileData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId))
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User ID."
                });
            }

            var res = await _userService.GetProfile(currentUserId);
            if(res.TryGetData(out var profile))
                return Ok(new SuccessResponse()
                {
                    Data = profile,
                    Message = res.Message
                });
            else
                return BadRequest(new ErrorResponse()
                {
                    Message = res.Message
                });
        }

       
        // Update user email
        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest emailRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId) || currentUserId != emailRequest.Id)
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid User ID."
                });
            }

            var res = await _userService.UpdateEmail(emailRequest);

            return res.ToActionResult();
        }


        [HttpPost("confirm-email-change")]
        public async Task<IActionResult> ConfirmEmailChange(ConfirmEmailRequest req)
        {
            var res = await _userService.ConfirmEmailChange(req);
            return res.ToActionResult();
        }


        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest req)
        {

            if (req.OldPassword == req.NewPassword)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Passwords cannot be the same"
                });

            var claimId = User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Token."
                });

            var res = await _userService.UpdatePassword(req, claimId);
            return res.ToActionResult();
        }


        [HttpPost("update-username")]
        public async Task<IActionResult> UpdateUsername(UpdateUsernameRequest req)
        {

            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null)
                return BadRequest(new ErrorResponse()
                {
                    Message = "invalid Id"
                });

            var res = await _userService.UpdateUsernameRequest(req, claimId);
            return res.ToActionResult();

        }


        [HttpPost("update-image")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile image, [FromForm] int id)
        {
            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null || claimId != id.ToString())
                return BadRequest(new ErrorResponse()
                {
                    Message = "Ids don't match"
                });

            if (!UploadingService.IsValidImageType(image))
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Image"
                });
            }

            try
            {
                var user = await _unitOfWork.UserRepo.GetUserWithFiles(id);

                if(user == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "User not found."
                    });

                using var imgStream = image.OpenReadStream();

                var dbFileHash = await _unitOfWork.UserRepo.GetUserImageHash(id);

                var imgHash = await _hashingService.HashWithxxHash(imgStream);

                if (dbFileHash.Hash == imgHash)
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "This image is the same as the old one."
                    });
                }
                
                imgStream.Position = 0;

                var imageName = "user_" + user.Id;

                FileHash? fileHash = await _uploadingService.UploadImageAsync(imgStream, imageName, CloudinaryType.UserImage);

                if (fileHash == null)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Image, Failed to upload."
                    });


                if(dbFileHash.PublicId == ICloudinaryService.DefaultUserImagePublicId)
                {
                    user.FileHashes.Remove(dbFileHash);
                    user.FileHashes.Add(fileHash);
                }
                else
                    dbFileHash.UpdateFromHash(fileHash);
                
                await _unitOfWork.SaveChangesAsync();


                return Ok(new SuccessResponse()
                {
                    Message = "Image Updated Successfully"
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something wrong happend."
                });
            }


        }




    }
}
