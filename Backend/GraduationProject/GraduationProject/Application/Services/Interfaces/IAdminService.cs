using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ServiceResult<bool>> BanUser(int id);
        Task<ServiceResult<bool>> UnbanUser(int id);
        Task<ServiceResult<PaginatedList<AppUserDTO>>> GetUsersPage(int index, string? role);
        Task<ServiceResult<bool>> VerifyInstructor(int id);


    }
}
