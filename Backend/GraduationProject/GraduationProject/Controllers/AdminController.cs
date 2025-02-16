using GraduationProject.Controllers.APIResponses;
using GraduationProject.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        public AdminController(ICourseRepository courseRepository, IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }


        [HttpGet("users")]
        public async Task<ActionResult> GetUsers()
        {
            return Ok(await _userRepository.GetUsersDTO());
        }

        // DELETE: api/admin/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(int id)
        {
            var banned = await _userRepository.BanAppUser(id);
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
