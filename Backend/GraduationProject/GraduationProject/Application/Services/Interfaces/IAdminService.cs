using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task BanUser(int id);
        Task UnbanUser(int id);
        Task<PaginatedList<AppUserDTO>> GetUsersPage(int index, string? role);
    }
}
