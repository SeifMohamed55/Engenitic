
using GraduationProject.Models;
using GraduationProject.Models.DTOs;

using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{


    public interface IUserRepository : IRepository<AppUser>
    {
        Task<AppUserDto?> GetUserByEmail(string email);
        Task<bool> DeleteRefreshToken(int id);
        Task<bool> UpdateRefreshToken(AppUser appUser, RefreshToken token);
    }

    public class AppUsersRepository : Repository<AppUser>, IUserRepository
    {

        public AppUsersRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AppUserDto>> GetUsers()
        {
            return await _appUsers
                .Include(x=> x.Roles)
                .Include(x=> x.RefreshToken)
                .Select(x=> new AppUserDto
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode =  x.PhoneRegionCode,
                    ImageURL = x.imageURL,
                    Roles = x.Roles.Select(x=> x.Name.ToLower())
                }).ToListAsync();
        }

        public async Task<AppUserDto?> GetAppUser(int id)
        {
            return await _appUsers
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Select(x => new AppUserDto
                {
                    Id = x.Id,
                    RefreshToken = x.RefreshToken.Token,
                    ExpiryDate = x.RefreshToken.ExpiryDate,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    ImageURL = x.imageURL,
                    Roles = x.Roles.Select(x => x.Name.ToLower())
                }).FirstOrDefaultAsync(x=> x.Id == id);
        }


        public async Task<bool> UpdateUser(AppUserDto dto)
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
            catch (Exception )
            {
            }
            return false;

        }


        public async Task<bool> DeleteAppUser(int id)
        {
            return await DeleteAsync(id);
        }

        public async Task<AppUserDto?> GetUserByEmail(string email)
        {
            return await _appUsers
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Select(x => new AppUserDto
                {
                    Id = x.Id,
                    RefreshToken = x.RefreshToken.Token,
                    ExpiryDate = x.RefreshToken.ExpiryDate,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    ImageURL = x.imageURL,
                    Roles = x.Roles.Select(x => x.Name.ToLower())
                }).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> UpdateRefreshToken(AppUser appUser, RefreshToken token)
        {
            try
            {
                appUser.RefreshToken = token;
                await UpdateAsync(appUser);
                return true;
            }
            catch (Exception )
            {

            }
            return false;

        }

        public async Task<bool> DeleteRefreshToken(int id)
        {
            try
            {
                var user = await _appUsers.FirstOrDefaultAsync(x => x.Id == id);
                if (user == null)
                {
                    return false;
                }
                user.RefreshToken = null;
                await UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {

            }
            return false;

        }
    }
}
