using BACKEND.CONFIGURATION;
using BACKEND.DOMAIN.Objects;
using BACKEND.SERVICES.AUTH.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BACKEND.SERVICES.AUTH
{
    public class TokenService : ITokenService
    {
        private JWTOptions _options;

        public TokenService(IOptions<JWTOptions> options)
        {
            _options = options.Value;

            Console.WriteLine("Signing key : "+ _options.SigningKey);

            if (string.IsNullOrWhiteSpace(_options.SigningKey) || Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:SigningKey must be configured and at least 32 bytes long. " +
                    "Set it in appsettings.json or an environment variable before starting the app.");
            }
        }

        public int AccessTokenMinutes => _options.AccessTokenMinutes;
        public int RefreshTokenDays => _options.RefreshTokenDays;

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("name", user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshTokenValue()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
