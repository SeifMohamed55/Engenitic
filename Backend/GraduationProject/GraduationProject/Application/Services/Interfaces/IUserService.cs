using GraduationProject.API.Requests;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<AppUserDTO>> GetProfile(int userId);
        Task<ServiceResult<bool>> UpdateEmail(UpdateEmailRequest req);
        Task<ServiceResult<bool>> ConfirmEmailChange(ConfirmEmailRequest req);
        Task<ServiceResult<bool>> UpdatePassword(UpdatePasswordRequest req, string claimId);
        Task<ServiceResult<bool>> UpdateUsernameRequest(UpdateUsernameRequest req, string claimId);
    }
}
