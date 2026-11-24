using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.AuthDto;

namespace SystemQMS.Services
{
    public class AuthSvc : IAuthSvc
    {
        private readonly MyDbContext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IEmailSvc _emailService;
        public AuthSvc(
            MyDbContext context, 
            IPasswordHasher<Users> passwordHasher,
            IConfiguration configuration,
            IMemoryCache cache,
            IEmailSvc emailService
         )
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _cache = cache;
            _emailService = emailService;
        }

        public RegisterDto Register(RegisterDto users)
        {
            if(_context.Users.Any(x => x.Username == users.userName))
            {
                throw new Exception("username already exists");
            }
            if(users.password != users.rePassword)
            {
                throw new Exception("password and confirm password do not match");
            }
            var user = new Users()
            {
                Name = users.name,
                Username = users.userName,
                Email = users.email,
                NumberPhone = users.numberPhone,
                RoleId = users.roleId,
                IsActive = true,
                CounterId = 1,
                UnitId = 1
            };

            // hash password
            user.Password = _passwordHasher.HashPassword(user, users.password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return users;
        }

        private string CreateToken(string tokenReturn, Users user)
        {
            string secretKey = _configuration["Jwt:Key"];
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            var securityKey = new SymmetricSecurityKey(secretKeyBytes);

            var signIn = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("Username", user.Username),
                new Claim("UnitId", user.UnitId.ToString()),
                new Claim("RoleId", user.RoleId.ToString()),
            };

            var token = new JwtSecurityToken(null, null, claims, null, DateTime.UtcNow.AddDays(1), signIn);
            tokenReturn = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenReturn;
        }
        public ResponceToken Login(LoginDto users)
        {
            var user = _context.Users.FirstOrDefault(x => x.Username == users.userName);
            if (user == null)
            {
                throw new Exception("username does not exist");
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, users.password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid username or password");
            }
            var token = CreateToken(string.Empty, user);
            return new ResponceToken { Token = token };
        }

        private class OtpCacheData
        {
            public string OtpHash { get; set; }
            public int Attempts { get; set; }
            public bool IsVerified { get; set; }
        }
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email))
                throw new Exception("Email is required");

            string email = dto.email.Trim().ToLowerInvariant();

            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Email.ToLower() == email
            );

            if (user == null)
                return;

            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            string otpHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(otp))
            );

            var otpData = new OtpCacheData
            {
                OtpHash = otpHash,
                Attempts = 0,
                IsVerified = false
            };

            _cache.Set(
                $"OTP_{email}",
                otpData,
                TimeSpan.FromMinutes(5)
            );

            string body = $@"
                    <div style='font-family:Arial;text-align:center'>
                         <h2>SystemQMS - Quên mật khẩu</h2>
                           <p>Mã xác nhận của bạn là:</p>
                    <h1 style='color:#2563eb;letter-spacing:5px'>
                                    {otp}
                        </h1>
                        <p>Mã OTP có hiệu lực trong 5 phút.</p>
                    <p>Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
                 </div>
                ";

            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "SystemQMS - Password Reset",
                    body
                );
            }
            catch
            {
                _cache.Remove($"OTP_{email}");
                throw;
            }
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email) ||
                string.IsNullOrWhiteSpace(dto.otp))
                return false;

            string email = dto.email.Trim().ToLowerInvariant();

            if (dto.otp.Length != 6 || !dto.otp.All(char.IsDigit))
                return false;

            string cacheKey = $"OTP_{email}";

            if (!_cache.TryGetValue<OtpCacheData>(
                cacheKey, out var otpData) || otpData == null)
                return false;

            // Nếu đã xác thực OTP thì không xác thực lại
            if (otpData.IsVerified)
                return true;

            string inputHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(dto.otp))
            );

            bool isValid = CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otpData.OtpHash),
                Convert.FromHexString(inputHash)
            );

            if (!isValid)
            {
                otpData.Attempts++;

                if (otpData.Attempts >= 5)
                    _cache.Remove(cacheKey);

                return false;
            }

            // Đánh dấu OTP đã xác thực
            otpData.IsVerified = true;

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email) ||
                string.IsNullOrWhiteSpace(dto.newPassword) ||
                string.IsNullOrWhiteSpace(dto.confirmPassword))
                return false;

            if (dto.newPassword != dto.confirmPassword)
                throw new Exception("Password confirmation does not match");

            string email = dto.email.Trim().ToLowerInvariant();

            string cacheKey = $"OTP_{email}";

            if (!_cache.TryGetValue<OtpCacheData>(
                cacheKey, out var otpData) ||
                otpData == null || !otpData.IsVerified)
                return false;

            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.Email.ToLower() == email
            );

            if (user == null)
                return false;

            user.Password = _passwordHasher.HashPassword(
                user,
                dto.newPassword
            );

            await _context.SaveChangesAsync();

            // Xóa OTP sau khi đổi mật khẩu
            _cache.Remove(cacheKey);

            return true;
        }
    }
}
