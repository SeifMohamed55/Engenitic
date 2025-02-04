using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUserDTO>> GetProfileData(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }

            if (int.TryParse(userId, out int currentUserId) && id != currentUserId)
            {
                return Forbid();
            }
            var appUser = await _appUsersRepository.GetAppUserDTO(id);

            if (appUser == null)
            {
                return NotFound();
            }

            return appUser;
        }
 

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAppUser(int id)
        {
            var banned = await _appUsersRepository.BanAppUser(id);
            if (banned)
            {
                return Ok();
            }

            return BadRequest();
        }

    }
}
