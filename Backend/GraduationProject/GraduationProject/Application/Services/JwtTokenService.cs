﻿using GraduationProject.API.Requests;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace GraduationProject.Application.Services
{

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly byte[] _jwtKey;
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> options,
            IEncryptionService encryptionService,
            IOptions<TokenValidationParameters> tokenValidationParameters)
        {
            _jwtKey = Encoding.UTF8.GetBytes(options.Value.Key);
            _encryptionService = encryptionService;
            _tokenValidationParameters = tokenValidationParameters.Value;
            _jwtOptions = options.Value;
        }

        // user included with roles and refreshToken
        public (string, string) GenerateJwtToken(AppUser user, List<string> roles)
        {

            var key = new SymmetricSecurityKey(_jwtKey);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var Jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Jti)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes)),
                signingCredentials: credentials); // privatekey

            var strToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (strToken, Jti);
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // Ignore token expiration
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = key
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtToken = securityToken as JwtSecurityToken;
            if (jwtToken == null)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }



        public bool IsAccessTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                tokenHandler.ValidateToken(token, _tokenValidationParameters, out securityToken);

                var jwtToken = securityToken as JwtSecurityToken;
                if (jwtToken == null)
                {
                    throw new SecurityTokenException("Invalid token");
                }
                return true; // Token is valid
            }
            catch (Exception)
            {
                return false; // Token validation failed
            }
        }

        // in caching
        public DateTimeOffset GetAccessTokenExpiration(string accessToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out securityToken);

            var jwtToken = securityToken as JwtSecurityToken;
            if (jwtToken == null)
            {
                throw new SecurityTokenException("Invalid token");
            }

            var expClaim = principal.Claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null && long.TryParse(expClaim.Value, out var expUnix))
            {
                // Convert Unix timestamp to DateTimeOffset
                return DateTimeOffset.FromUnixTimeSeconds(expUnix);
            }
            else
            {
                throw new SecurityTokenException("Invalid token");
            }
        }

        public (int, string) ExtractIdAndJtiFromExpiredToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var jtiClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            if (idClaim?.Value != null && jtiClaim?.Value != null)
            {
                if (int.TryParse(idClaim.Value, out int id))
                    return (id, jtiClaim.Value);
            }
            throw new SecurityTokenException("Invalid token structure");
        }
        public string? ExtractJwtTokenFromContext(HttpContext context)
        {
            return context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        }

        public bool VerifyRefresh(string raw, string hashed)
        {
            return _encryptionService.AesDecrypt(hashed) == raw;
        }

        public bool IsRefreshTokenExpired(RefreshToken refreshToken)
        {
            return refreshToken.ExpiresAt.ToUniversalTime() <= DateTimeOffset.UtcNow;
        }
    }
}
