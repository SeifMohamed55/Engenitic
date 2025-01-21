using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using GraduationProject.Controllers.ApiRequest;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;


namespace GraduationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<Role> _roleManager;

        public RolesController(AppDbContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }


        // PUT: api/Roles
        [HttpPut()]
        public async Task<IActionResult> EditRole
                    ([FromBody]RoleRequest request)
        {
            if (request.NewName == null)
                return BadRequest("Please provide new name");

            if (request.OldId == 0 && request.OldName == null)
                return BadRequest("Please provide either id or name");

            else if (request.OldId != 0)
            {
                var role = await _roleManager.FindByIdAsync(request.OldId.ToString());
                if (role == null)
                    return NotFound();

                var res = await _roleManager.SetRoleNameAsync(role, request.NewName);
                if (res.Succeeded)
                    res = await _roleManager.UpdateAsync(role);
                    if (!res.Succeeded)
                        return BadRequest(res.Errors);
            }
            else if (request.OldName != null)
            {
                var role = await _roleManager.FindByNameAsync(request.OldName);
                if (role == null)
                    return NotFound();

                var res = await _roleManager.SetRoleNameAsync(role, request.NewName);
                if (res.Succeeded)
                    res = await _roleManager.UpdateAsync(role);
                    if(!res.Succeeded)
                        return BadRequest(res.Errors);
            }
            else
            {
                return BadRequest();
            }
            return Ok();
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole([FromBody] RoleRequest request)
        {
            if(request.NewName == null)
                return BadRequest("Please provide new name");

            Role newRole = new Role { Name = request.NewName};
            var x =  await _roleManager.CreateAsync(newRole);
            if(x.Succeeded)
                return CreatedAtAction("GetRole", new { id = newRole.Id }, newRole);
            else
                return BadRequest(x.Errors);

        }

        // DELETE: api/Roles
        [HttpDelete()]
        public async Task<IActionResult> DeleteRole([FromBody] RoleRequest request)
        { 
            int id = request.OldId;
            string? name = request.OldName;

            if (id == 0 && name == null)
                return BadRequest("Please provide either id or name");

            else if(id != 0)
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                    return NotFound();

                var res = await _roleManager.DeleteAsync(role);

                if(!res.Succeeded)
                    return BadRequest(res.Errors);
            }
            else if(name != null)
            {
                var role = await _roleManager.FindByNameAsync(name);
                if (role == null)
                    return NotFound();

                var res = await _roleManager.DeleteAsync(role);

                if (!res.Succeeded)
                    return BadRequest(res.Errors);
            }
            else
            {
                BadRequest();
            }

            return Ok();
        }

        [HttpDelete("deleteDatabase")]
        public void DeleteDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }
    }
}
