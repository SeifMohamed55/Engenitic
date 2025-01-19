namespace SpringBootCloneApp.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Threading.Tasks;
    using SpringBootCloneApp.Models;
    using SpringBootCloneApp.Services;
    using Microsoft.AspNetCore.Identity.Data;
    using SpringBootCloneApp.Controllers.RequestModels;
    using SpringBootCloneApp.Data;
    using SpringBootCloneApp.Models.Enums;
    using SpringBootCloneApp.Models.DTOs;
    using NuGet.Packaging;
    using System.Security.Claims;
    using Microsoft.EntityFrameworkCore;
    using NuGet.Common;
    using SpringBootCloneApp.Controllers.ResponseModels;
    using Microsoft.AspNetCore.Authorization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Diagnostics;
    using SpringBootCloneApp.Attributes;
    using Microsoft.Extensions.Options;
    using SpringBootCloneApp.StartupConfigurations;
    using Microsoft.AspNetCore.Authentication.Google;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using System.Configuration.Provider;
    using Microsoft.DotNet.Scaffolding.Shared;
    using Mono.TextTemplating;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<Client> _userManager;
        private readonly ILoginService _loginService;
        public AuthenticationController(
            UserManager<Client> userManager,
            ILoginService loginService)

        {
            _userManager = userManager;
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
        public  IResult Revoke()
        {
            return _loginService.Logout(HttpContext);
        }


        [HttpPost("register")]
        [SkipJwtTokenMiddleware]
        public async Task<IActionResult> Register(RegisterCustomRequest model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new Client()
            {
                Email = model.Email,
                Address = model.Address,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);


            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, Role.ROLE_USER.ToString());

                if (result.Succeeded)
                    return Ok(new { result = "User created successfully", user = new ClientDTO(user) });
            }

            return BadRequest(result.Errors);
        }

        
    }
}



