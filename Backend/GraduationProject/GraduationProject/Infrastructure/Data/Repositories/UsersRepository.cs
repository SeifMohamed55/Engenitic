using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface IUserRepository : IRepository<AppUser>
    {
        Task<PaginatedList<AppUserDTO>> GetUsersDTO(int index);
        Task<List<AppUser>> GetAllUsersAsync();
        Task<AppUserDTO?> GetAppUserDTO(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(int id);
        Task<AppUser?> GetUserWithTokenAndRoles(string email);
        Task<RefreshToken?> GetUserRefreshToken(int id);
        void UpdateUser(AppUserDTO dto);
        void UpdateUserLatestToken(AppUser user, string latestToken);
        Task BanAppUser(int id);
        Task<AppUserDTO?> GetUserDTOByEmail(string email);
        void UpdateRefreshToken(AppUser appUser, RefreshToken token);
        Task<FileHash> GetUserImageHash(int id);

        //Task<string?> GetUserImage(int id);

    }

    public class UsersRepository : Repository<AppUser>, IUserRepository
    {

        private readonly JwtOptions _jwtOptions;

        public UsersRepository(AppDbContext context, IOptions<JwtOptions> options) : base(context)
        {
            _jwtOptions = options.Value;
        }

        public async Task<PaginatedList<AppUserDTO>> GetUsersDTO(int index)
        {
            var query =  _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Include(x=> x.FileHashes)
                .DTOProjection();

            return await PaginatedList<AppUserDTO>.CreateAsync(query, index);
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _dbSet
                .ToListAsync();
        }

        public async Task<AppUserDTO?> GetAppUserDTO(int id)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .DTOProjection()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /*public async Task<string?> GetUserImage(int id)
        {
            return (await _dbSet.FindAsync(id))?.ImageSrc;
        }*/

        // Needs optimization
        public async Task<AppUser?> GetUserWithTokenAndRoles(int id)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Include(x => x.FileHashes)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserWithTokenAndRoles(string email)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Include(x => x.FileHashes)
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
            var user = await GetByIdAsync(dto.Id);
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
            if (user == null)
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
            if (entry != null)
                entry.State = EntityState.Detached; // don't track it

            appUser.RefreshToken = token;
            Update(appUser);

        }

        public async Task<FileHash> GetUserImageHash(int userId)
        {
            return (await _dbSet
                 .Include(x => x.FileHashes)
                 .Select(x => new
                 {
                     x.Id,
                     FileHash = x.FileHashes.First(x => x.Type == CloudinaryType.UserImage)
                 })
                .FirstAsync(x => x.Id == userId)
                ).FileHash;

        }

    }
}
