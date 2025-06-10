using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IUserRepository : IBulkRepository<AppUser, int>, ICustomRepository
    {
        Task<AppUserDTO?> GetAppUserDTO(int id);
        Task<AppUser?> GetUserWithRoles(int id);
        Task<AppUser?> GetUserWithRoles(string email);
        Task<FileHash> GetUserImageHash(int id);
        Task<AppUser?> GetUserWithFiles(int id);
        Task<PaginatedList<AppUserDTO>> GetBannedUsersDTO(int index);
        Task<PaginatedList<AppUserDTO>> GetUsersInRolePage(int index, Role? role);

        //Task<string?> GetUserImage(int id);

    }
}
