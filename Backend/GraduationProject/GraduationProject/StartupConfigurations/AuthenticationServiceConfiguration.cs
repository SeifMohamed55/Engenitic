using GraduationProject.Controllers.APIResponses;
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
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
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
                options.ClockSkew = TimeSpan.Zero;
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
                         if (context.Request.Cookies.ContainsKey("access_token"))
                         {
                             context.Token = context.Request.Cookies["access_token"];
                         }
                         return Task.CompletedTask;
                     },
                     OnChallenge = context =>
                     {
                         context.HandleResponse(); // Suppress default challenge behavior

                         context.Response.StatusCode = 401;
                         context.Response.ContentType = "application/json";
                         string msg = context.Request.Headers["Authorization"]
                                        .FirstOrDefault()?
                                        .Split(" ")
                                        .Last() != null ?
                                        "Unauthorized Please Refresh Access Token" :
                                        "Unauthorized No Access Token Provided";

                         return context.Response.WriteAsJsonAsync(new ErrorResponse
                         {
                             Message = msg,
                             Code = System.Net.HttpStatusCode.Unauthorized,
                         });
                     },
                     OnForbidden = context =>
                     {
                         context.Response.StatusCode = 403;
                         context.Response.ContentType = "application/json";
                         
                        return context.Response.WriteAsJsonAsync(new ErrorResponse
                        {
                            Message = "Forbidden Insufficient Roles",
                            Code = System.Net.HttpStatusCode.Forbidden,
                        });
                     }
                 };
             })
            .AddCookie()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Mailing:ClientId"] ?? "";
                googleOptions.ClientSecret = configuration["Authentication:Mailing:ClientSecret"] ?? "";
                googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                googleOptions.AccessType = "offline";
                googleOptions.SaveTokens = true;
                // ✅ Force Google to ask for consent and return a refresh token
                googleOptions.AccessDeniedPath = "/api/google/AccessDeniedPathInfo";
            })
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
