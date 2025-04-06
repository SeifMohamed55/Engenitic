using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
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
        private readonly ILoginRegisterService _loginService;
        private readonly IAdminService _adminService;
        public AdminController(
            ILoginRegisterService loginRegisterService,
            IAdminService adminService)
        {
            _loginService = loginRegisterService;
            _adminService = adminService;
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

            try
            {
                var data = await _adminService.GetUsersPage(index, role);

                if (index > data.TotalPages && data.TotalPages != 0)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Data = data,
                    Message = "Users retrieved successfully."
                });
            }
            catch (InvalidParameterException ex)
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Something Wrong occured"
                });
            }
        }

        // DELETE: api/admin/ban/5
        [HttpDelete("ban/{id}")]
        public async Task<IActionResult> BanAppUser(int id)
        {
            try
            {
                await _adminService.BanUser(id);

                return Ok(new SuccessResponse()
                {
                    Message = "User has been banned.",
                    Code = HttpStatusCode.OK
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "User not found.",
                    Code = HttpStatusCode.BadRequest
                });
            }
        }

        // DELETE: api/admin/ban/5
        [HttpDelete("unban/{id}")]
        public async Task<IActionResult> UnbanAppUser(int id)
        {
            try
            {
                await _adminService.UnbanUser(id);

                return Ok(new SuccessResponse()
                {
                    Message = "User has been banned.",
                    Code = HttpStatusCode.OK
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Message = "User not found.",
                    Code = HttpStatusCode.BadRequest
                });
            }
        }



        [HttpPost("register")]
        public async Task<IActionResult> AddAdmin([FromForm] RegisterCustomRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = string.Join('\n', ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)),
                    Status = "Error",
                });

            model.Role = "admin";
            var adminResult = await _loginService.Register(model, false);

            if (adminResult.IsSuccess)
                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Data = adminResult.Data,
                    Message = "Admin has been added successfully."
                });
            else
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = adminResult.Message ?? "",
                });
        }


    }
}
