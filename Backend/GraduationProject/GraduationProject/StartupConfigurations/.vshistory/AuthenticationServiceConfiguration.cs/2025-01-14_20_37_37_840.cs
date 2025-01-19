using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GraduationProject.StartupConfigurations
{
    public static class AuthenticationServiceConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            //var signingKey = await new SigningIssuerCertficate().GetIssuerSigningKey(); // RSA
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)//signingKey
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/notificationHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
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
