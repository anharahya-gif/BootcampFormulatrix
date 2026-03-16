using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PokerAPIMultiplayerWithDB.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(int playerId, int expiryMinutes = 60);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secret;

        public JwtTokenService()
        {
            _secret = Environment.GetEnvironmentVariable("DOTNET_JWT_KEY") ?? string.Empty;
        }

        public string GenerateToken(int playerId, int expiryMinutes = 60)
        {
            if (string.IsNullOrEmpty(_secret)) throw new InvalidOperationException("JWT secret not configured in DOTNET_JWT_KEY");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("playerId", playerId.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
