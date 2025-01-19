namespace GraduationProject.StartupConfigurations
{
    public class JwtOptions
    {
        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string Subject { get; set; }
        public required string AccessTokenValidityMinutes { get; set; }
        public required string RefreshTokenValidityDays { get; set; }

    }
}
