using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(int userId, string username, string role)
        {
            string key = _configuration["Jwt:Key"]
                ?? throw new Exception("Thiếu Jwt:Key");

            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new Exception("Thiếu Jwt:Issuer");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new Exception("Thiếu Jwt:Audience");

            double duration = double.TryParse(
                _configuration["Jwt:Duration"],
                out double minutes) ? minutes : 120;

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}