using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Security;
using System.Net;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthenticationService _loginService;
        private readonly IAdminService _adminService;
        private readonly ICloudinaryService _cloudinaryService;
        public AdminController(
            IAuthenticationService loginRegisterService,
            IAdminService adminService,
            ICloudinaryService cloudinaryService)
        {
            _loginService = loginRegisterService;
            _adminService = adminService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] int index = 1)
        {
            if (index < 1)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid index."
                });

            var res = await _adminService.GetUsersPage(index, role);

            if (res.TryGetData(out var data))
            {
               data.ForEach(user =>
               {
                   user.Image.ImageURL = _cloudinaryService.GetImageUrl(user.Image.ImageURL, user.Image.Version);
                   user.Cv.ImageURL = _cloudinaryService.GetPDF(user.Cv.ImageURL, user.Cv.Version);
               });
            }

            return res.ToActionResult();
  
        }

        // POST: api/admin/ban/5
        [HttpPost("ban/{id}")]
        public async Task<IActionResult> BanAppUser(int id)
        {
            var res = await _adminService.BanUser(id);

            return res.ToActionResult();
        }

        // POST: api/admin/ban/5
        [HttpPost("unban/{id}")]
        public async Task<IActionResult> UnbanAppUser(int id)
        {
            var res = await _adminService.UnbanUser(id);
            return res.ToActionResult();
        }

        [HttpPost("verify-instructor/{id}")]
        public async Task<IActionResult> VerifyInstructor(int id)
        {
            var res = await _adminService.VerifyInstructor(id);
            return res.ToActionResult();
        }



        [HttpPost("register")]
        public async Task<IActionResult> AddAdmin([FromForm] RegisterCustomRequest model)
        {
            model.Role = "admin";
            var adminResult = await _loginService.Register(model, false);

            return adminResult.ToActionResult();
        }



    }
}
