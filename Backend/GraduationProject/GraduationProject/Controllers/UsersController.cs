﻿using GraduationProject.Controllers.ApiRequest;
using GraduationProject.Controllers.APIResponses;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _appUsersRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGmailServiceHelper _emailService;
        private readonly JwtOptions _jwtOptions;

        public UsersController(
            IUserRepository appUsersRepository,
            UserManager<AppUser> userManager,
            IGmailServiceHelper emailService,
            IOptions<JwtOptions> options)
        {
            _appUsersRepository = appUsersRepository;
            _userManager = userManager;
            _emailService = emailService;
            _jwtOptions = options.Value;
        }

        // GET: api/Users/
        [HttpGet("profile")]
        public async Task<ActionResult<AppUserDTO>> GetProfileData([FromQuery] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId) || currentUserId != id)
            {
                return Unauthorized(new ErrorResponse() {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Invalid User ID."
                });
            }

            var appUser = await _appUsersRepository.GetAppUserDTO(currentUserId);

            if (appUser == null)
            {
                return NotFound(new ErrorResponse()
                {
                    Code = HttpStatusCode.NotFound,
                    Message = "User not found."
                });
            }

            return Ok(new SuccessResponse()
            {
                Data = appUser,
                Message = "User fetched successfully!"
            });
        }

        // GET: api/Users/image
        [HttpGet("image")]
        public async Task<ActionResult> GetUserImage([FromQuery] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse() {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "User ID not found."
                });
            }
            if (!int.TryParse(userId, out int currentUserId) || currentUserId != id)
            {
                return Unauthorized(new ErrorResponse()
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Invalid User ID."
                });
            }
            var userImage = await _appUsersRepository.GetUserImage(currentUserId);
            if (userImage == null)
            {
                return NotFound(new ErrorResponse()
                {
                    Code = HttpStatusCode.NotFound,
                    Message = "User image not found."
                });
            }

            string imagePath = Path.Combine(Directory.GetCurrentDirectory(),
                                                            "uploads", "images", "users", userImage);
            try
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                var fileExtension = Path.GetExtension(imagePath).ToLower();

                return File(imageBytes, ImageHelper.GetImageType(fileExtension));
            }
            catch
            {
                return NotFound(new ErrorResponse
                {
                    Code = HttpStatusCode.NotFound,
                    Message = "Image Not found"
                });
            }


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

                var emailExists = (await _userManager.FindByEmailAsync(emailRequest.NewEmail)) != null;
                if (emailExists)
                    return BadRequest(new ErrorResponse()
                    {
                        Code= HttpStatusCode.BadRequest,
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
                return BadRequest("User not found.");
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
                Message = "Email verified successfully"
            });

        }



        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code= HttpStatusCode.BadRequest,
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
            if(!res.Succeeded)
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
            if(user == null)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "User does not exist."
                });

            user.FullName = req.NewUsername;
            var res  = await _userManager.UpdateAsync(user);
            if(!res.Succeeded)
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
            if (!ImageHelper.IsValidImageType(image))
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid Image Format or size."
                });

            var claimId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (claimId == null || claimId != id.ToString())
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Ids don't match"
                });

            var res = await _appUsersRepository.UpdateUserImage(image, id);
            if (!res)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Something wrong happend."
                });

            return Ok(new SuccessResponse()
            {
                Code = HttpStatusCode.OK,
                Message = "Image Updated Successfully"
            });
        }



    }
}
