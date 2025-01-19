namespace GraduationProject.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Threading.Tasks;
    using GraduationProject.Controllers.RequestModels;

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILoginService _loginService;
        public LoginController(
            UserManager<AppUser> userManager,
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



