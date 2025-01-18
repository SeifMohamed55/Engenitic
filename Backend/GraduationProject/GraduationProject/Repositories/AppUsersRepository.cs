
using GraduationProject.Models.DTOs;

using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{


    public interface IUserRepository : IRepository<AppUser>
    {
        Task<AppUser?> GetUserByEmail(string email); 
    }

    public class AppUsersRepository : Repository<AppUser>, IUserRepository
    {

        public AppUsersRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            return await GetAllAsync();
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

        public async Task<bool> AddUser(AppUser appUser)
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

        public Task<AppUser?> GetUserByEmail(string email)
        {
           return _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
