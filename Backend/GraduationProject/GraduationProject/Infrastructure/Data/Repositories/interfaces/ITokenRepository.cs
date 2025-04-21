using GraduationProject.API.Requests;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface ITokenRepository : IGenericRepository<RefreshToken>, ICustomRepository
    {
        void DeleteRefreshToken(Guid deviceId);
        Task<RefreshToken?> GetUserRefreshToken(Guid deviceId, int userId);
        RefreshToken GenerateRefreshToken(int userId, DeviceInfo deviceInfo);

        Task RemoveRevokedOrExpiredByUserId(int id);
    }
}
