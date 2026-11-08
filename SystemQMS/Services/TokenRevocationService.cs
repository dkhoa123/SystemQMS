using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using SystemQMS.Models;
using SystemQMS.Models.Password;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly MyDbContext _context;

        public TokenRevocationService(MyDbContext context)
        {
            _context = context;
        }

        // Thu hồi token khi người dùng đăng xuất
        public async Task<bool> RevokeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            string tokenHash = TokenHelper.HashToken(token);

            bool exists = await _context.revokedTokens
                .AnyAsync(x => x.TokenHash == tokenHash);

            if (exists)
                return true;

            // Đọc thời gian hết hạn (ValidTo / exp) trực tiếp từ JWT Token
            DateTime expiresAt = DateTime.UtcNow.AddHours(2); // Mốc dự phòng nếu parse lỗi
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    expiresAt = jwtToken.ValidTo;
                }
            }
            catch
            {
                // Giữ mốc mặc định nếu không đọc được chuỗi JWT
            }

            var revokedToken = new RevokedToken
            {
                TokenHash = tokenHash,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt // Gán giá trị để thỏa mãn ràng buộc NOT NULL của PostgreSQL
            };

            _context.revokedTokens.Add(revokedToken);

            await _context.SaveChangesAsync();

            return true;
        }

        // Kiểm tra token đã bị thu hồi chưa
        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            string tokenHash = TokenHelper.HashToken(token);

            return await _context.revokedTokens
                .AnyAsync(x => x.TokenHash == tokenHash);
        }
    }
}