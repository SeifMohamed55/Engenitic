
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Services;
using GraduationProject.StartupConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;

namespace GraduationProject.Repositories
{

    public interface IUserRepository
    {
        Task<IEnumerable<AppUserDTO>> GetUsersDTO();
        Task<AppUserDTO?> GetAppUserDTO(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(string email);
        Task<RefreshToken?> GetUserRefreshToken(int id);
        Task<bool> UpdateUser(AppUserDTO dto);
        Task UpdateUserLatestToken(AppUser user, string latestToken);
        Task<bool> BanAppUser(int id);
        Task<AppUserDTO?> GetUserDTOByEmail(string email);
        Task<bool> UpdateRefreshToken(AppUser appUser, RefreshToken token);
        Task<bool> DeleteRefreshToken(int id);
        Task UpdateUserImage(AppUser user, string image);
        Task<bool> EnrollOnCourse(int userId, int courseId);
        Task<string?> GetUserImage(int id);

    }

    public class AppUsersRepository : Repository<AppUser>, IUserRepository
    {

        private readonly AppDbContext _context;
        private readonly JwtOptions _jwtOptions;

        public AppUsersRepository(AppDbContext context, IOptions<JwtOptions> options) : base(context)
        {
            _context = context;
            _jwtOptions = options.Value;
        }

        public async Task<IEnumerable<AppUserDTO>> GetUsersDTO()
        {
            return await _dbSet
                .Include(x=> x.Roles)
                .Include(x=> x.RefreshToken)
                .Select(x=> new AppUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode =  x.PhoneRegionCode,
                    Image = new ImageMetadata() { Name = x.ImageSrc, ImageURL = "https://localhost/api/users/image" },
                }).ToListAsync();
        }

        public async Task<AppUserDTO?> GetAppUserDTO(int id)
        {
            return await _dbSet 
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Select(x => new AppUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    Image = new() { Name = x.ImageSrc, ImageURL = "https://localhost/api/users/image"},
                    UserName = x.FullName,
                    Banned = x.Banned
                }).FirstOrDefaultAsync(x=> x.Id == id);
        }

        public async Task<string?> GetUserImage(int id)
        {
            return (await _dbSet.FindAsync(id))?.ImageSrc;
        }

        public async Task<AppUser?> GetUserWithTokenAndRoles(int id)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserWithTokenAndRoles(string email)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<RefreshToken?> GetUserRefreshToken(int id)
        {
            return (await _dbSet
                .Include(x => x.RefreshToken)
                .FirstOrDefaultAsync(x => x.Id == id))?.RefreshToken;
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
            catch (Exception )
            {
            }
            return false;

        }

        public async Task UpdateUserLatestToken(AppUser user, string latestJti)
        {
            if (user.RefreshToken == null)
                throw new ArgumentNullException();

            user.RefreshToken.LatestJwtAccessTokenJti = latestJti;
            user.RefreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                                                        (double.Parse(_jwtOptions.AccessTokenValidityMinutes));
            await UpdateAsync(user);
        }


        public async Task<bool> BanAppUser(int id)
        {
            var user = await GetByIdAsync(id);
            if(user == null)
            {
                return false;
            }

            user.Banned = true;
            try
            {
                await UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        public async Task<AppUserDTO?> GetUserDTOByEmail(string email)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Select(x => new AppUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    Image = new ImageMetadata() { Name = x.ImageSrc, ImageURL = "https://localhost/api/users/image" }
                }).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> UpdateRefreshToken(AppUser appUser, RefreshToken token)
        {
            try
            {
                var entry = _dbSet.Entry(appUser).Context.ChangeTracker.Entries<RefreshToken>().FirstOrDefault();
                if(entry != null)
                    entry.State = EntityState.Detached; // don't track it

                appUser.RefreshToken = token;
                await UpdateAsync(appUser);
                return true;
            }
            catch (Exception)
            {

            }
            return false;

        }

        public async Task<bool> DeleteRefreshToken(int id)
        {
            try
            {
                var user = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
                if (user == null || user.RefreshToken == null)
                {
                    return false;
                }

                _context.RefreshTokens.Remove(user.RefreshToken);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
            }
            return false;

        }


        public async Task UpdateUserImage(AppUser user, string image)
        {
            user.ImageSrc = image;
            await UpdateAsync(user);
        }

        public async Task<bool> EnrollOnCourse(int userId, int courseId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            try
            {
                user.Enrollments.Add(new UserEnrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    IsCompleted = false,
                    CurrentStage = 1
                });
                await UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


    }
}
