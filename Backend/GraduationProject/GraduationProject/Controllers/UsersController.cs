using GraduationProject.Controllers.APIResponses;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _appUsersRepository;

        public UsersController(IUserRepository appUsersRepository)
        {
            _appUsersRepository = appUsersRepository;
        }

        // GET: api/Users/
        [HttpGet("profile")]
        public async Task<ActionResult<AppUserDTO>> GetProfileData([FromQuery] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int currentUserId) || currentUserId != id)
            {
                return Unauthorized(new ErrorResponse(){
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
        [Authorize]
        public async Task<ActionResult> GetUserImage([FromQuery] int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse(){
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



    }
}
