using GraduationProject.Controllers.RequestModels;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Services;
using GraduationProject.Controllers.APIResponses;

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
        public async Task<IResult> Login(LoginCustomRequest model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = "Invalid Data",
                    Status = "Error"
                });

            return await _loginService.Login(model, HttpContext);

        }


        // not authorized endpoint
        [HttpPost("logout")]      
        public async Task<IResult> Revoke()
        {
            return await _loginService.Logout(HttpContext);
        }


        [HttpPost("register")]
        public async Task<IResult> Register([FromForm]RegisterCustomRequest model)
        {

            if (!ModelState.IsValid)
                return Results.BadRequest(new ErrorResponse(){
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = ModelState,
                    Status = "Error",
                });
            

            return await _loginService.Register(model);

        }

    }
}

