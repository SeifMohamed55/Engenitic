using GraduationProject.API.Requests;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using GraduationProject.StartupConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public class TokenRepository : Repository<RefreshToken>, ITokenRepository
    {
        private readonly JwtOptions _jwtOptions;
        public TokenRepository(AppDbContext context, IOptions<JwtOptions>jwtOptions) : base(context)
        {
            _jwtOptions = jwtOptions.Value;
        }

        private Guid GenerateSecureToken()
        {
            var randomNumber = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return new Guid(randomNumber);
        }

        public RefreshToken GenerateRefreshToken(int userId, DeviceInfo deviceInfo)
        {
            var refreshToken =  new RefreshToken()
            {
                DeviceId = deviceInfo.DeviceId,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays)),
                LoginProvider = _jwtOptions.Issuer,
                Token = GenerateSecureToken(),
                IpAddress = deviceInfo.IpAddress,
                UserAgent = deviceInfo.UserAgent,
                UserId = userId,
            };

            _dbSet.Add(refreshToken);
            return refreshToken;
        }

        public void DeleteRefreshToken(Guid deviceId)
        {
            Delete(deviceId);
        }

        public async Task<RefreshToken?> GetUserRefreshToken(Guid deviceId, int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.DeviceId == deviceId && rt.UserId == userId);
        }

        public async Task RemoveRevokedOrExpiredByUserId(int id)
        {
            await _dbSet
                .Where(x => x.UserId == id && (x.IsRevoked || x.ExpiresAt < DateTimeOffset.UtcNow))
                .ExecuteDeleteAsync();
        }
    }
}
