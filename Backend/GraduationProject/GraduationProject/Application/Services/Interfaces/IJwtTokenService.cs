using GraduationProject.Domain.Models;


namespace GraduationProject.Application.Services.Interfaces
{
    public interface IJwtTokenService
    {
        (string, string) GenerateJwtToken(AppUser userWithTokenAndRoles, List<string> roles);
        (int, string) ExtractIdAndJtiFromExpiredToken(string token);
        bool IsAccessTokenValid(string token);
        string? ExtractJwtTokenFromContext(HttpContext context);
        DateTimeOffset GetAccessTokenExpiration(string accessToken);
        bool IsRefreshTokenExpired(RefreshToken refreshToken);
        bool VerifyRefresh(string raw, string hashed);
    }
}
