using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;

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
            return await DeleteAsync(id);
        }

        public async Task<bool> Exists(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }

    }
}
