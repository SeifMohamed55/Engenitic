﻿using GraduationProject.Controllers.RequestModels;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Services;
using GraduationProject.Controllers.APIResponses;
using Microsoft.AspNetCore.RateLimiting;

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
        //[EnableRateLimiting("UserLoginRateLimit")]
        public async Task<IResult> Login(LoginCustomRequest model)
        {
            if (!ModelState.IsValid)
                return Results.BadRequest(new ErrorResponse()
                {
                    Code = System.Net.HttpStatusCode.BadRequest,
                    Message = ModelState,
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
                });
            

            return await _loginService.Register(model);

        }

    }
}

