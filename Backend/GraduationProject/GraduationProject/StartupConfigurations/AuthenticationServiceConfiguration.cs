using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GraduationProject.StartupConfigurations
{
    public static class AuthenticationServiceConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            services.Configure<TokenValidationParameters>(options =>
            {
                options.ValidateIssuer = tokenValidationParameters.ValidateIssuer;
                options.ValidateAudience = tokenValidationParameters.ValidateAudience;
                options.ValidateLifetime = tokenValidationParameters.ValidateLifetime;
                options.ValidateIssuerSigningKey = tokenValidationParameters.ValidateIssuerSigningKey;
                options.ValidIssuer = tokenValidationParameters.ValidIssuer;
                options.ValidAudience = tokenValidationParameters.ValidAudience;
                options.IssuerSigningKey = tokenValidationParameters.IssuerSigningKey;
            });                                                                                       

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = tokenValidationParameters;
             })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
                googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                googleOptions.AccessDeniedPath = "/api/google/AccessDeniedPathInfo";
            })
            .AddCookie()
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = configuration["Authentication:Facebook:AppId"] ?? "";
                facebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? "";
                facebookOptions.AccessDeniedPath = "/api/facebook/AccessDeniedPathInfo";
            });

            return services;
        }
    }
}
