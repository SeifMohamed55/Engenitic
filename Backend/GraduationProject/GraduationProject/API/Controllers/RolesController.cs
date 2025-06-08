using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;


namespace GraduationProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;

        public RolesController(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult> GetRoles()
        {
            try
            {
                return Ok(new SuccessResponse
                {
                    Message = "Roles retrieved successfully",
                    Code = HttpStatusCode.OK,
                    Data = await _roleManager.Roles.ToListAsync(),
                });
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Message = "An error occurred while retrieving roles",
                    Code = HttpStatusCode.InternalServerError,
                });
            }
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Role not found",
                    Code = HttpStatusCode.NotFound,
                });
            }

            return Ok(new SuccessResponse
            {
                Message = "Role retrieved successfully",
                Code = HttpStatusCode.OK,
                Data = role
            });
        }


        // PUT: api/Roles
        [HttpPut()]
        public async Task<IActionResult> EditRole([FromBody] RoleRequest request)
        {
            //Edit BadRequest to  new ErrorResponse

            if (request.NewName == null)
                return BadRequest(new ErrorResponse
                {
                    Message = "Please provide new name",
                    Code = HttpStatusCode.BadRequest,
                });

            if (request.OldId == 0 && request.OldName == null)
                return BadRequest(new ErrorResponse
                {
                    Message = "Please provide either name or id",
                    Code = HttpStatusCode.BadRequest,
                });

            else if (request.OldId != 0)
            {
                var role = await _roleManager.FindByIdAsync(request.OldId.ToString());
                if (role == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Role not found",
                        Code = HttpStatusCode.NotFound,
                    });

                var res = await _roleManager.SetRoleNameAsync(role, request.NewName);
                if (res.Succeeded)
                    res = await _roleManager.UpdateAsync(role);
                if (!res.Succeeded)
                    return BadRequest(new ErrorResponse
                    {
                        Message = string.Join('\n', res.Errors.Select(x=> x.Description)),
                        Code = HttpStatusCode.BadRequest,
                    });
            }
            else if (request.OldName != null)
            {
                var role = await _roleManager.FindByNameAsync(request.OldName);
                if (role == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Role not found",
                        Code = HttpStatusCode.NotFound,
                    });

                var res = await _roleManager.SetRoleNameAsync(role, request.NewName);
                if (res.Succeeded)
                    res = await _roleManager.UpdateAsync(role);
                if (!res.Succeeded)
                    return BadRequest(new ErrorResponse
                    {
                        Message = string.Join('\n', res.Errors.Select(x => x.Description)),
                        Code = HttpStatusCode.BadRequest,
                    });
            }
            else
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "An error occurred",
                    Code = HttpStatusCode.BadRequest,
                });
            }
            return Ok(new SuccessResponse
            {
                Message = "Role updated successfully",
                Code = HttpStatusCode.OK,
            });
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole([FromBody] RoleRequest request)
        {
            if (request.NewName == null)
                return BadRequest(new ErrorResponse
                {
                    Message = "Please provide a new name",
                    Code = HttpStatusCode.BadRequest,
                });

            Role newRole = new Role { Name = request.NewName };
            var x = await _roleManager.CreateAsync(newRole);
            if (x.Succeeded)
                return CreatedAtAction("GetRole", new { id = newRole.Id }, newRole);
            else
                return BadRequest(new ErrorResponse { Message = string.Join('\n', x.Errors.Select(x => x.Description)), Code = HttpStatusCode.BadRequest });

        }

        // DELETE: api/Roles
        [HttpDelete()]
        public async Task<IActionResult> DeleteRole([FromBody] RoleRequest request)
        {
            int id = request.OldId;
            string? name = request.OldName;

            if (id == 0 && name == null)
                return BadRequest(new ErrorResponse { Message = "Please provide either id or name", Code = HttpStatusCode.BadRequest });

            else if (id != 0)
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Role not found",
                        Code = HttpStatusCode.NotFound,
                    });

                var res = await _roleManager.DeleteAsync(role);

                if (!res.Succeeded)
                    return BadRequest(new ErrorResponse
                    {
                        Message = string.Join('\n', res.Errors.Select(x => x.Description)),
                        Code = HttpStatusCode.BadRequest
                    });
            }
            else if (name != null)
            {
                var role = await _roleManager.FindByNameAsync(name);
                if (role == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Role not found",
                        Code = HttpStatusCode.NotFound,
                    });

                var res = await _roleManager.DeleteAsync(role);

                if (!res.Succeeded)
                    return BadRequest(new ErrorResponse
                    {
                        Message = string.Join('\n', res.Errors.Select(x => x.Description)),
                        Code = HttpStatusCode.BadRequest
                    });
            }
            else
            {
                BadRequest(new ErrorResponse
                {
                    Message = "An error occurred",
                    Code = HttpStatusCode.BadRequest,
                });
            }

            return Ok(new SuccessResponse
            {
                Message = "Role deleted successfully",
                Code = HttpStatusCode.OK,
            });
        }
    }
}
