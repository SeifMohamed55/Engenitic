﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

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
                     OnTokenValidated = context =>
                     {
                         var token = context.SecurityToken as JwtSecurityToken;

                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = context =>
                     {
                         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                         {
                             Console.WriteLine("Token expired");
                         }
                         return Task.CompletedTask;
                     },

                     OnChallenge = context =>
                     {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(new { Message = "Unauthorized" });
                        return context.Response.WriteAsync(result);
                     },
                     OnForbidden = context =>
                     {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = JsonSerializer.Serialize(new { Message = "Forbidden" });
                        return context.Response.WriteAsync(result);
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
