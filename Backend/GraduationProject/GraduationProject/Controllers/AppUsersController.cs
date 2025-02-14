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

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetUsers()
        {
            return Ok(await _appUsersRepository.GetUsersDTO());
        }

        // GET: api/Users/
        [HttpGet("profile")]
        public async Task<ActionResult<AppUserDTO>> GetProfileData()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || int.TryParse(userId, out int currentUserId))
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

            return appUser;
        }

        // GET: api/Users/image
        [HttpGet("image")]
        [Authorize]
        public async Task<ActionResult> GetUserImage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse(){
                    Code = HttpStatusCode.Unauthorized,
                    Message = "User ID not found." 
                });
            }
            if (!int.TryParse(userId, out int currentUserId))
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
                                                            "uploads", "images", userImage);
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            var fileExtension = Path.GetExtension(imagePath).ToLower();

            return File(imageBytes, ImageHelper.GetImageType(fileExtension));

        }

        // GET: Enrolled Courses Page
        [HttpGet("enrolled-courses/{index}")]
        [Authorize(Roles= "student")]
        public async Task<ActionResult> GetEnrolledCoursesPage(int index)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new ErrorResponse() {
                    Code = HttpStatusCode.Unauthorized, 
                    Message = "User ID not found." });
            }
            if (!int.TryParse(userId, out int currentUserId))
            {
                return Unauthorized(new ErrorResponse() {
                    Code = HttpStatusCode.Unauthorized,
                    Message = "Invalid User ID." 
                });
            }
            return Ok(await _appUsersRepository.GetEnrolledCoursesPage(index, currentUserId));
        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAppUser(int id)
        {
            var banned = await _appUsersRepository.BanAppUser(id);
            if (banned)
            {
                return Ok(new SuccessResponse()
                {
                    Message = "User has been banned.",
                    Code = HttpStatusCode.OK
                });
            }

            return BadRequest(new ErrorResponse()
            {
                Message = "User not found.",
                Code = HttpStatusCode.BadRequest
            });
        }

    }
}
