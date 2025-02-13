
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;

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
        Task<bool> DeleteRefreshToken(AppUser appUser);
        Task UpdateUserImage(AppUser user, string image);
        Task<bool> EnrollOnCourse(int userId, int courseId);
        Task<PaginatedList<EnrollmentDTO>> GetEnrolledCoursesPage(int index, int userId);
        Task<string?> GetUserImage(int id);
    }

    public class AppUsersRepository : Repository<AppUser>, IUserRepository
    {

        private readonly DbSet<UserEnrollment> _enrollments;

        public AppUsersRepository(AppDbContext context) : base(context)
        {
            _enrollments = context.Set<UserEnrollment>();
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
                    ImageURL = x.ImageSrc,
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
                    ImageURL = x.ImageSrc,
                    UserName = x.FullName
                }).FirstOrDefaultAsync(x=> x.Id == id);
        }

        public async Task<string?> GetUserImage(int id)
        {
            return (await _dbSet.FindAsync(id))?.ImageSrc;
        }

        public async Task<PaginatedList<EnrollmentDTO>> GetEnrolledCoursesPage(int index, int userId)
        {
            var Enrollments = _enrollments
                .Where(x=> x.UserId == userId)
                .OrderBy(x=> x.CourseId)
                .Select(e => new EnrollmentDTO(e));
            return await PaginatedList<EnrollmentDTO>.CreateAsync(Enrollments, index);

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

        public async Task UpdateUserLatestToken(AppUser user, string latestToken)
        {
            if (user.RefreshToken == null)
                throw new ArgumentNullException();

            user.RefreshToken.LatestJwtAccessToken = latestToken;
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
                    ImageURL = x.ImageSrc,
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

        public async Task<bool> DeleteRefreshToken(AppUser appUser)
        {
            try
            {
                appUser.RefreshToken = null;
                await UpdateAsync(appUser);
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
