namespace GraduationProject.Application.Services.Interfaces
{
    public interface ITokenBlacklistService
    {
        bool IsTokenBlacklisted(string accessToken);
        bool IsTokenBlacklisted(HttpContext context);
        void BlacklistToken(string accessTokenJti, DateTimeOffset exp);
    }


}
