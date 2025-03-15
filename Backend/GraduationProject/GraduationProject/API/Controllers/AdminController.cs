using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoginRegisterService _loginService;
        public AdminController(
            IUnitOfWork unitOfWork,
            ILoginRegisterService loginRegisterService)
        {
            _loginService = loginRegisterService;
            _unitOfWork = unitOfWork;
        }


        [HttpGet("users")]
        public async Task<ActionResult> GetUsers([FromQuery] int index = 1)
        {
            if (index < 1)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid index."
                });

            try
            {
                var data = await _unitOfWork.UserRepo.GetUsersDTO(index);

                if (data.TotalPages < index)
                    return BadRequest(new ErrorResponse()
                    {
                        Code = HttpStatusCode.BadRequest,
                        Message = "Invalid Index"
                    });

                return Ok(new SuccessResponse()
                {
                    Code = HttpStatusCode.OK,
                    Data = data,
                    Message = "Users retrieved successfully."
                });
            }
            catch
            {
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "No users found."
                });
            }
        }

/*        // DELETE: api/admin/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> BanAppUser(int id)
        {
            try
            {
                await _unitOfWork.UserRepo.BanAppUser(id);
                await _unitOfWork.SaveChangesAsync();

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
*/
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
                    Message = adminResult.Error ?? "",
                });
        }


    }
}
