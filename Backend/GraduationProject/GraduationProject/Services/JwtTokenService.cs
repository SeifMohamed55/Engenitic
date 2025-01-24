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
        string GenerateJwtToken(AppUser client);
        int? ExtractIdFromExpiredToken(string token);

    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IAesEncryptionService _aesService;

        public JwtTokenService(IOptions<JwtOptions> options, IAesEncryptionService aesService)
        {
            _jwtOptions = options.Value;
            _aesService = aesService;
        }

        public string GenerateJwtToken(AppUser user)
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
            var encryptedToken = _aesService.Encrypt(Guid.NewGuid().ToString());
            var refreshToken = new RefreshToken()
             {
                 ExpiryDate = DateTimeOffset.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidityDays)),
                 Id = appUser.RefreshTokenId ?? 0,
                 LoginProvider = _jwtOptions.Issuer,
                 EncryptedToken = encryptedToken

            };

            return refreshToken;
             
        }

        private  ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
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

        public int? ExtractIdFromExpiredToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var idClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            int id;
            if (idClaim?.Value != null)
            {
              var res = int.TryParse(idClaim.Value, out id);
                if (res)
                    return id;
            }
            return null;
        }


    }
}
