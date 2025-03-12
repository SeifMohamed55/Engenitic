using Microsoft.Extensions.Caching.Memory;

namespace GraduationProject.Application.Services
{
    public interface ITokenBlacklistService
    {
        bool IsTokenBlacklisted(string accessToken);
        bool IsTokenBlacklisted(HttpContext context);
        void BlacklistToken(string accessTokenJti, DateTimeOffset exp);
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
            (int _, string accessTokenJti) = _jwtTokenService.ExtractIdAndJtiFromExpiredToken(accessToken);
            return _memoryCache.TryGetValue(accessTokenJti, out _);
        }

        public bool IsTokenBlacklisted(HttpContext context)
        {
            var accessToken = _jwtTokenService.ExtractJwtTokenFromContext(context);
            if (accessToken == null)
            {
                return false;
            }
            (int _, string accessTokenJti) = _jwtTokenService.ExtractIdAndJtiFromExpiredToken(accessToken);
            return _memoryCache.TryGetValue(accessTokenJti, out _);
        }

        public void BlacklistToken(string accessTokenJti, DateTimeOffset exp)
        {
            try
            {
                _memoryCache.Set(accessTokenJti, true, exp);

            }
            catch (Exception)
            {
            }
        }
    }


}
