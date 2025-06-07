using Microsoft.IdentityModel.Tokens;
using Presentation.Interfaces;
using Presentation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Created with GPT4o
// Setups a secret signing key and asserts it is not null
// Signing creadentials are created using the key and HMAC SHA256
// Requires claims such as user ID, email adn username
// One claims type role for identity auth and one for a borader range like external systems and external APIs
// Create the token with the claims, expiration time, and signing credentials
// Serialize the token to a string and return it

namespace Presentation.Services
{

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _cfg;
        public JwtTokenGenerator(IConfiguration cfg) => _cfg = cfg;


        public Task<string> CreateTokenAsync(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_cfg["Jwt:SigningKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.Name,               user.UserName!)
        };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            claims.AddRange(roles.Select(r => new Claim("role", r)));

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
