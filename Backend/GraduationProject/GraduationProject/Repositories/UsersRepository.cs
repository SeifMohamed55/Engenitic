using GraduationProject.Data;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.StartupConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GraduationProject.Repositories
{

    public interface IUserRepository
    {
        Task<IEnumerable<AppUserDTO>> GetUsersDTO();
        Task<AppUserDTO?> GetAppUserDTO(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(string email);
        Task<RefreshToken?> GetUserRefreshToken(int id);
        void UpdateUser(AppUserDTO dto);
        void UpdateUserLatestToken(AppUser user, string latestToken);
        Task BanAppUser(int id);
        Task<AppUserDTO?> GetUserDTOByEmail(string email);
        void UpdateRefreshToken(AppUser appUser, RefreshToken token);
        Task<string?> GetUserImage(int id);

    }

    public class UsersRepository : Repository<AppUser>, IUserRepository
    {

        private readonly JwtOptions _jwtOptions;

        public UsersRepository(AppDbContext context, IOptions<JwtOptions> options) : base(context)
        {
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
                .DTOProjection()
                .FirstOrDefaultAsync(x=> x.Id == id);
        }

        public async Task<string?> GetUserImage(int id)
        {
            return (await _dbSet.FindAsync(id))?.ImageSrc;
        }

        // Needs optimization
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

        public async void UpdateUser(AppUserDTO dto)
        {
            var user  = await GetByIdAsync(dto.Id);
            if (user == null)
            {
                throw new ArgumentNullException("User does not exist");
            }
            user.UpdateFromDTO(dto);
            Update(user);
        }

        public void UpdateUserLatestToken(AppUser user, string latestJti)
        {
            if (user.RefreshToken == null)
                throw new ArgumentNullException();

            user.RefreshToken.LatestJwtAccessTokenJti = latestJti;
            user.RefreshToken.LatestJwtAccessTokenExpiry = DateTime.UtcNow.AddMinutes
                                                        (double.Parse(_jwtOptions.AccessTokenValidityMinutes));
            Update(user);
        }


        public async Task BanAppUser(int id)
        {
            var user = await GetByIdAsync(id);
            if(user == null) 
                throw new ArgumentNullException(nameof(user));
            
            user.Banned = true;

            Update(user);

        }

        public async Task<AppUserDTO?> GetUserDTOByEmail(string email)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .DTOProjection()
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public void UpdateRefreshToken(AppUser appUser, RefreshToken token)
        {

            var entry = _dbSet.Entry(appUser).Context.ChangeTracker.Entries<RefreshToken>().FirstOrDefault();
            if(entry != null)
                entry.State = EntityState.Detached; // don't track it

            appUser.RefreshToken = token;
            Update(appUser);
 
        }


        
    }
}
