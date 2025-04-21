using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface ILoginRegisterService
    {
        Task<ServiceResult<LoginWithCookies>> Login(LoginCustomRequest model, DeviceInfo deviceInfo);
        Task<ServiceResult<RefreshToken>> Logout(Guid deviceId, int userId);
        Task<ServiceResult<LoginWithCookies>> ExternalLogin(string provider, AuthenticatedPayload payload);
        Task<ServiceResult<AppUserDTO>> Register(RegisterCustomRequest model, bool isExternal);
    }
}
