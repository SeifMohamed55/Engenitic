using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{


    public interface IUserRepository : IRepository<AppUser>
    {

    }

    public class AppUsersRepository : Repository<AppUser>, IUserRepository
    {

        public AppUsersRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            return await this._context.Users.ToListAsync();
        }

        public async Task<AppUser> GetAppUser(int id)
        {
            var appUser = await _dbContext.Users.FindAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            return appUser;
        }


        public async Task<IActionResult> PutAppUser(int id, AppUser appUser)
        {
            if (id != appUser.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(appUser).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        public async Task<AppUser> AddAppUser(AppUser appUser)
        {
            _dbContext.Users.Add(appUser);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetAppUser", new { id = appUser.Id }, appUser);
        }


        public async Task<bool> DeleteAppUser(int id)
        {
            var appUser = await _dbContext.Users.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }

            _dbContext.Users.Remove(appUser);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool AppUserExists(int id)
        {
            return _dbContext.Users.Any(e => e.Id == id);
        }
    }
}
