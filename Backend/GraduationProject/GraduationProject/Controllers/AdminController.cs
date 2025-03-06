using GraduationProject.Controllers.APIResponses;
using GraduationProject.Controllers.RequestModels;
using GraduationProject.Data;
using GraduationProject.Repositories;
using GraduationProject.Services;
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
        public async Task<ActionResult> GetUsers()
        {
            return Ok(await _unitOfWork.UserRepo.GetUsersDTO());
        }

        // DELETE: api/admin/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(int id)
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

        [HttpPost("register")]
        public async Task<IResult> AddAdmin([FromForm]RegisterCustomRequest model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = ModelState,
                    Status = "Error",
                });


            return await _loginService.RegisterAdmin(model);
        }


    }
}
