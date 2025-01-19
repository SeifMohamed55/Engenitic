using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Mono.TextTemplating;
using SpringBootCloneApp.Models;
using SpringBootCloneApp.StartupConfigurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SpringBootCloneApp.Services
{
    public interface IJwtTokenService
    {
        RefreshToken GenerateRefreshToken();
        Task<string?> GenerateAsymmetricJwtToken(Client client);
        string GenerateSymmetricJwtToken(Client client);
        long? ExtractIdFromExpiredToken(string token);

    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IRsaCertficate _signingIssuerCertficate;

        public JwtTokenService(IOptions<JwtOptions> options, IRsaCertficate signingIssuerCertficate)
        {
            _jwtOptions = options.Value;
            _signingIssuerCertficate = signingIssuerCertficate;
        }

        public async Task<string?> GenerateAsymmetricJwtToken(Client user)
        {

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach(var role in user.Authorities)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
            
            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidity)),
                signingCredentials: await _signingIssuerCertficate.GetAudienceSigningKey()); // privatekey

            var strToken = new JwtSecurityTokenHandler().WriteToken(token);

            return strToken;
        }

        public string GenerateSymmetricJwtToken(Client user)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in user.Authorities)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_jwtOptions.AccessTokenValidity)),
                signingCredentials: credentials); // privatekey

            var strToken = new JwtSecurityTokenHandler().WriteToken(token);

            return strToken;
        }

        public RefreshToken GenerateRefreshToken()
        {
             var refreshToken = new RefreshToken()
             {
                 ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_jwtOptions.RefreshTokenValidity)),
                 Value = Guid.NewGuid().ToString("N"),
                 Name = "local",
                 LoginProvider = "localhost",
             };

            return refreshToken;
             
        }


        public async Task<ClaimsPrincipal> GetAsymmetricPrincipalFromExpiredToken(string token)
        {
            var signingKey = await _signingIssuerCertficate.GetIssuerSigningKey();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // Ignore token expiration
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = signingKey
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
