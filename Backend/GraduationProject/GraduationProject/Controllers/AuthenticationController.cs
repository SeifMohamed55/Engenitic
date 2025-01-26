using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Services;
using Ganss.Xss;

namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoginRegisterService _loginService;
        public AuthenticationController(ILoginRegisterService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("login")]
        //[SkipJwtTokenMiddleware]
        public async Task<IResult> Login(LoginCustomRequest model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);

            return await _loginService.Login(model, HttpContext);

        }



        [HttpPost("logout")]      
        public async Task<IResult> Revoke()
        {
            return await _loginService.Logout(HttpContext);
        }


        [HttpPost("register")]
        public async Task<IResult> Register(RegisterCustomRequest model/*, [FromForm] IFormFile file*/)
        {

            if (!ModelState.IsValid)
                return Results.BadRequest(ModelState);
            

            return await _loginService.Register(model);

        }

    }
}

