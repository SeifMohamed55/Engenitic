using GraduationProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mono.TextTemplating;
using GraduationProject.StartupConfigurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GraduationProject.Services
{
    public interface IJwtTokenService
    {
        RefreshToken GenerateRefreshToken(AppUser appUser);
        string GenerateSymmetricJwtToken(AppUser client);
        long? ExtractIdFromExpiredToken(string token);

    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value;
        }

        public string GenerateSymmetricJwtToken(AppUser user)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidityMinutes)),
                signingCredentials: credentials); // privatekey

            var strToken = new JwtSecurityTokenHandler().WriteToken(token);

            return strToken;
        }

        public RefreshToken GenerateRefreshToken(AppUser appUser)
        {
             var refreshToken = new RefreshToken()
             {
                 ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays)),
                 Token = Guid.NewGuid().ToString("N"),
                 LoginProvider = _jwtOptions.Issuer,
                 Id = appUser.RefreshTokenId ?? 0
             };

            return refreshToken;
             
        }

        public  ClaimsPrincipal GetSymmetricPrincipalFromExpiredToken(string token)
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

        public long? ExtractIdFromExpiredToken(string token)
        {
            var principal =  GetSymmetricPrincipalFromExpiredToken(token);
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            long id;
            if (idClaim?.Value != null)
            {
              var res = long.TryParse(idClaim.Value, out id);
                if (res)
                    return id;
            }
            return null;
        }


    }
}
