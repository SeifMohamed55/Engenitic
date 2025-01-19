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
            return await GetUsers();
        }

        public async Task<AppUser?> GetAppUser(int id)
        {
            var appUser = await GetByIdAsync(id);

            return appUser;
        }


        public async Task<IActionResult> PutAppUser(AppUserDTO appUser)
        {
           
        }


        public async Task<bool> AddAppUser(AppUser appUser)
        {
            try
            {
                await AddAsync(appUser);
                return true;
            } 
            catch (Exception e)
            {
                return false;
            }

        }


        public async Task<bool> DeleteAppUser(int id)
        {
            var appUser = await _context.Users.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }

            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
