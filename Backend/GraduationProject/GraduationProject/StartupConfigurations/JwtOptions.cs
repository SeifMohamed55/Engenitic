namespace GraduationProject.StartupConfigurations
{
    public class JwtOptions
    {
        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; } // api consumer 
        public required string AccessTokenValidityMinutes { get; set; }
        public required string RefreshTokenValidityDays { get; set; }
        public required string RefreshTokenKey { get; set; }
        public required string IV { get; set; }


    }
}
