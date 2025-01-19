using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Models.DTOs;
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


        public async Task<bool> UpdateUser(AppUserDTO dto)
        {
            var user  = await GetByIdAsync(dto.Id);
            if (user == null)
            {
                return false;
            }
            try
            {
                user.UpdateFromDTO(dto);
                await UpdateAsync(user);
                return true;

            }
            catch (Exception e)
            {
            }
            return false;

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

    }
}
