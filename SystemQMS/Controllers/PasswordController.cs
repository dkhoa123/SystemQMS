
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SystemQMS.Models;
using SystemQMS.Models.Password;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.Login;
using SystemQMS.ViewModels.Password;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IEmailService _emailService;

        private readonly PasswordHasher<Guest> _passwordHasher = new();

        public PasswordController(
            MyDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Lấy Guest.id từ JWT
        private bool TryGetGuestId(out int guestId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("nameid")?.Value
                ?? User.FindFirst("UserId")?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out guestId);
        }

        // Hash OTP
        private static string HashOtp(string otp)
        {
            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
        }

        // POST: api/Password/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var guest = await _context.guests
                .FirstOrDefaultAsync(x =>
                    x.email != null &&
                    x.email.ToLower() == email);

            if (guest == null)
            {
                return NotFound(new
                {
                    message = "Email chưa được đăng ký!"
                });
            }

            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var oldOtps = await _context.passwordResetOtps
                .Where(x => x.Email == email
                    && x.Purpose == "ForgotPassword"
                    && !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            var passwordOtp = new PasswordResetOtp
            {
                Email = email,
                OtpHash = HashOtp(otp),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false,
                Purpose = "ForgotPassword"
            };

            _context.passwordResetOtps.Add(passwordOtp);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(email, otp);

            return Ok(new
            {
                message = "Mã OTP đã được gửi về email của bạn!",
                expiresIn = 300
            });
        }

        // POST: api/Password/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] PasswordVerifyOtpRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var otpData = await _context.passwordResetOtps
                .Where(x => x.Email == email
                    && x.Purpose == "ForgotPassword"
                    && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpData == null)
            {
                return BadRequest(new
                {
                    message = "Không tìm thấy OTP hoặc OTP đã được sử dụng!"
                });
            }

            if (otpData.ExpiresAt < DateTime.UtcNow)
            {
                otpData.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Mã OTP đã hết hạn!"
                });
            }

            string inputHash = HashOtp(request.Otp.Trim());

            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otpData.OtpHash),
                Convert.FromHexString(inputHash)))
            {
                return BadRequest(new
                {
                    message = "Mã OTP không chính xác!"
                });
            }

            otpData.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xác thực OTP thành công!"
            });
        }

        // POST: api/Password/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var guest = await _context.guests
                .FirstOrDefaultAsync(x =>
                    x.email != null &&
                    x.email.ToLower() == email);

            if (guest == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản!"
                });
            }

            var otpData = await _context.passwordResetOtps
                .Where(x => x.Email == email
                    && x.Purpose == "ForgotPassword"
                    && !x.IsUsed
                    && x.IsVerified)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpData == null)
            {
                return BadRequest(new
                {
                    message = "Bạn cần xác thực OTP trước!"
                });
            }

            if (otpData.ExpiresAt < DateTime.UtcNow)
            {
                otpData.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Mã OTP đã hết hạn!"
                });
            }

            string inputHash = HashOtp(request.Otp.Trim());

            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otpData.OtpHash),
                Convert.FromHexString(inputHash)))
            {
                return BadRequest(new
                {
                    message = "Mã OTP không chính xác!"
                });
            }

            guest.password = _passwordHasher.HashPassword(
                guest,
                request.NewPassword);

            otpData.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đặt lại mật khẩu thành công!"
            });
        }

        // POST: api/Password/change/send-otp
        [Authorize]
        [HttpPost("change/send-otp")]
        public async Task<IActionResult> SendChangePasswordOtp(
            [FromBody] ChangePasswordRequest request)
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản!"
                });
            }

            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null || string.IsNullOrWhiteSpace(guest.email))
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản hoặc email!"
                });
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                guest,
                guest.password,
                request.CurrentPassword);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return BadRequest(new
                {
                    message = "Mật khẩu hiện tại không chính xác!"
                });
            }

            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            string email = guest.email.Trim().ToLowerInvariant();

            var oldOtps = await _context.passwordResetOtps
                .Where(x => x.Email == email
                    && x.Purpose == "ChangePassword"
                    && !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            var passwordOtp = new PasswordResetOtp
            {
                Email = email,
                OtpHash = HashOtp(otp),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false,
                Purpose = "ChangePassword"
            };

            _context.passwordResetOtps.Add(passwordOtp);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(email, otp);

            return Ok(new
            {
                message = "OTP đổi mật khẩu đã được gửi đến email của bạn!",
                expiresIn = 300
            });
        }

        // POST: api/Password/change/confirm
        [Authorize]
        [HttpPost("change/confirm")]
        public async Task<IActionResult> ConfirmChangePassword(
            [FromBody] ComfirmChangePasswordRequest request)
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản!"
                });
            }

            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null || string.IsNullOrWhiteSpace(guest.email))
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản!"
                });
            }

            string email = guest.email.Trim().ToLowerInvariant();

            var otpData = await _context.passwordResetOtps
                .Where(x => x.Email == email
                    && x.Purpose == "ChangePassword"
                    && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpData == null)
            {
                return BadRequest(new
                {
                    message = "Không tìm thấy OTP. Vui lòng yêu cầu gửi OTP mới!"
                });
            }

            if (otpData.ExpiresAt < DateTime.UtcNow)
            {
                otpData.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "OTP đã hết hạn!"
                });
            }

            string inputHash = HashOtp(request.Otp.Trim());

            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(otpData.OtpHash),
                Convert.FromHexString(inputHash)))
            {
                return BadRequest(new
                {
                    message = "OTP không chính xác!"
                });
            }

            guest.password = _passwordHasher.HashPassword(
                guest,
                request.NewPassword);

            otpData.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đổi mật khẩu thành công!"
            });
        }
    }
}
