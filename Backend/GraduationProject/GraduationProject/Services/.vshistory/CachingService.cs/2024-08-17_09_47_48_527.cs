using Microsoft.Extensions.Caching.Memory;
using SpringBootCloneApp.Models;

namespace SpringBootCloneApp.Services
{
    public interface ICachingService
    {
        string? GetRefreshToken(string accessToken);
        void AddRefreshTokenCachedData(string accessToken, RefreshToken refreshToken);
        void RemoveCachedRefreshToken(string accessToken);
    }

    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _cache;

        public CachingService(IMemoryCache cache)
        {
            _cache = cache;

        }

        public string? GetRefreshToken(string accessToken)
        {
            if (_cache.TryGetValue(accessToken, out string? refreshToken))
                return refreshToken;

            return null;


        }

        public void AddRefreshTokenCachedData(string accessToken, RefreshToken refreshToken)
        {
            var cachedRefreshToken = refreshToken.Value;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = refreshToken.ExpiryDate

            };

            _cache.Set(accessToken, cachedRefreshToken, cacheEntryOptions);

        }

        public void RemoveCachedRefreshToken(string accessToken)
        {
            _cache.Remove(accessToken);
        }
    }
}
