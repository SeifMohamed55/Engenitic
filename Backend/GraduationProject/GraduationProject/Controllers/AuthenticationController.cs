using GraduationProject.Controllers.RequestModels;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringBootCloneApp.Services;
using Ganss.Xss;

namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILoginRegisterService _loginService;
        private readonly RoleManager<Role> _roleManager;
        public AuthenticationController(
            UserManager<AppUser> userManager,
            ILoginRegisterService loginService,
            RoleManager<Role> roleManager)

        {
            _userManager = userManager;
            _loginService = loginService;
            _roleManager = roleManager;
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
        public IResult Revoke()
        {
            return _loginService.Logout(HttpContext);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCustomRequest model, [FromForm]IFormFile file)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if(model.Password != model.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            string? RegionCode = null;
            if (model.PhoneNumber != null)
            {
                (string, string)? phoneDetails = Utilities.IsValidPhoneNumber(model.PhoneNumber);
                if(phoneDetails.HasValue)
                {
                    model.PhoneNumber = phoneDetails.Value.Item1;
                    RegionCode = phoneDetails.Value.Item2;
                }
                else
                {
                    return BadRequest("Invalid phone number");
                }
            }

            var userRole = await _roleManager.FindByNameAsync(model.Role);
            if (userRole == null || userRole.NormalizedName == "ADMIN" || userRole.Id == 1)
                return BadRequest();

            HtmlSanitizer sanitizer = new HtmlSanitizer();
            model.Username = sanitizer.Sanitize(model.Username);
            model.Email = sanitizer.Sanitize(model.Email);
            model.PhoneNumber = sanitizer.Sanitize(model.PhoneNumber ?? "");

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                Banned = false,
                PhoneRegionCode = RegionCode,
                imageURL = model.imageURL,

            };

            var result = await _userManager.CreateAsync(user, model.Password);


            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, model.Role);

                if (result.Succeeded)
                    return Ok(new
                    {
                        result = "User created successfully",
                        user = new AppUserDTO()
                        {
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber ?? "",
                            Id = user.Id,
                            imageURL = user.imageURL,
                            RegionCode = user.PhoneRegionCode
                        }
                    });
            }

            return BadRequest(result.Errors);
        }


    }
}

