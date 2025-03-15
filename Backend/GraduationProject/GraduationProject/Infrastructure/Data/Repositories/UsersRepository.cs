using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        Task<AppUserDTO?> GetUserDTOByEmail(string email);
        Task<FileHash> GetUserImageHash(int id);

        //Task<string?> GetUserImage(int id);

    }

    public class UsersRepository : Repository<AppUser>, IUserRepository
    {


        public UsersRepository(AppDbContext context) : base(context)
        {
        }

        private IQueryable<AppUser> DefaultQuery()
        {
            return _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .Include(x => x.FileHashes);
        }

        public async Task<PaginatedList<AppUserDTO>> GetUsersDTO(int index)
        {
            var query =  DefaultQuery()
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
            return await DefaultQuery()
                .DTOProjection()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserWithTokenAndRoles(int id)
        {
            return await DefaultQuery()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser?> GetUserWithTokenAndRoles(string email)
        {
            return await DefaultQuery()
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<RefreshToken?> GetUserRefreshToken(int id)
        {
            return (await _dbSet
                .Include(x => x.RefreshToken)
                .FirstOrDefaultAsync(x => x.Id == id))?.RefreshToken;
        }


        public async Task<AppUserDTO?> GetUserDTOByEmail(string email)
        {
            return await _dbSet
                .Include(x => x.Roles)
                .Include(x => x.RefreshToken)
                .DTOProjection()
                .FirstOrDefaultAsync(x => x.Email == email);
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
