using Microsoft.Extensions.Caching.Memory;
using GraduationProject.Models;
using Microsoft.Extensions.Options;
using GraduationProject.StartupConfigurations;

namespace GraduationProject.Services
{
    public interface ITokenBlacklistService
    {
        bool IsTokenBlacklisted(string accessToken);
        void BlacklistToken(string accessToken);
    }


    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IJwtTokenService _jwtTokenService;

        public TokenBlacklistService(IMemoryCache memoryCache, IJwtTokenService jwtTokenService)
        {
            _memoryCache = memoryCache;
            _jwtTokenService = jwtTokenService;
        }

        public bool IsTokenBlacklisted(string accessToken)
        {
            return _memoryCache.TryGetValue(accessToken, out _);
        }

        public void BlacklistToken(string accessToken)
        {
            try
            {
                var exp = _jwtTokenService.GetAccessTokenExpiration(accessToken);
                _memoryCache.Set(accessToken, true, exp);

            }
            catch(Exception)
            {
            }
        }
    }


}
