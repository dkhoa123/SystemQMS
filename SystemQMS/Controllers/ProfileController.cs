
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SystemQMS.Models;
using SystemQMS.Models.Password;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.Profile;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Guest")]
    public class ProfileController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IEmailService _emailService;

        public ProfileController(
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
                ?? User.FindFirst("UserId")?.Value;

            return int.TryParse(idValue, out guestId);
        }

        // Hash OTP
        private static string HashOtp(string otp)
        {
            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
        }

        // Kiểm tra OTP
        private static bool VerifyOtp(string inputOtp, string storedHash)
        {
            try
            {
                byte[] inputHash = Convert.FromHexString(HashOtp(inputOtp));
                byte[] originalHash = Convert.FromHexString(storedHash);

                return CryptographicOperations.FixedTimeEquals(
                    inputHash,
                    originalHash);
            }
            catch
            {
                return false;
            }
        }

        // GET: api/Profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản Guest."
                });
            }

            var guest = await _context.guests
                .AsNoTracking()
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            return Ok(new
            {
                guest.id,
                guest.guestId,
                guest.guestName,
                guest.email,
                guest.phoneNumber,
                guest.CCCD,
                guest.unitId,
                unitName = guest.Unit != null
                    ? guest.Unit.unitName
                    : null,
                guest.registerTime,
                guest.status
            });
        }

        // PUT: api/Profile/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản Guest."
                });
            }

            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Dữ liệu không hợp lệ."
                });
            }

            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            // Không cập nhật email ở API này.
            // Muốn đổi email phải xác thực OTP.
            guest.guestName = request.username?.Trim();
            guest.phoneNumber = request.numberPhone?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thông tin thành công."
            });
        }

        // POST: api/Profile/change-email/send-otp
        [HttpPost("change-email/send-otp")]
        public async Task<IActionResult> SendChangeEmailOtp(
            [FromBody] ChangeEmailRequest request)
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản Guest."
                });
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.NewEmail))
            {
                return BadRequest(new
                {
                    message = "Email mới không được để trống."
                });
            }

            string newEmail = request.NewEmail.Trim().ToLowerInvariant();

            if (newEmail.Length > 100 ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(newEmail))
            {
                return BadRequest(new
                {
                    message = "Email mới không hợp lệ."
                });
            }

            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null || string.IsNullOrWhiteSpace(guest.email))
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản hoặc email hiện tại."
                });
            }

            string currentEmail = guest.email.Trim().ToLowerInvariant();

            if (newEmail == currentEmail)
            {
                return BadRequest(new
                {
                    message = "Email mới phải khác email hiện tại."
                });
            }

            bool emailExists = await _context.guests.AnyAsync(x =>
                x.id != guestId &&
                x.email != null &&
                x.email.ToLower() == newEmail);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Email này đã được sử dụng."
                });
            }

            // Vô hiệu hóa OTP đổi email cũ
            var oldOtps = await _context.passwordResetOtps
                .Where(x =>
                    x.Email == currentEmail &&
                    x.Purpose == "ChangeEmail" &&
                    !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            string otp = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var otpData = new PasswordResetOtp
            {
                Email = currentEmail,
                NewEmail = newEmail,
                Purpose = "ChangeEmail",
                OtpHash = HashOtp(otp),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false
            };

            _context.passwordResetOtps.Add(otpData);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpAsync(newEmail, otp);
            }
            catch
            {
                otpData.IsUsed = true;
                await _context.SaveChangesAsync();

                return StatusCode(500, new
                {
                    message = "Gửi OTP thất bại. Vui lòng thử lại."
                });
            }

            return Ok(new
            {
                message = "Mã OTP đã được gửi đến email mới.",
                expiresIn = 300
            });
        }

        // POST: api/Profile/change-email/confirm
        [HttpPost("change-email/confirm")]
        public async Task<IActionResult> ConfirmChangeEmail(
            [FromBody] ConfirmChangeEmailRequest request)
        {
            if (!TryGetGuestId(out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản Guest."
                });
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.NewEmail) ||
                string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new
                {
                    message = "Email mới và OTP không được để trống."
                });
            }

            string newEmail = request.NewEmail.Trim().ToLowerInvariant();

            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null || string.IsNullOrWhiteSpace(guest.email))
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            string currentEmail = guest.email.Trim().ToLowerInvariant();

            var otpData = await _context.passwordResetOtps
                .Where(x =>
                    x.Email == currentEmail &&
                    x.NewEmail == newEmail &&
                    x.Purpose == "ChangeEmail" &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpData == null)
            {
                return BadRequest(new
                {
                    message = "Không tìm thấy OTP hoặc OTP đã được sử dụng."
                });
            }

            if (otpData.ExpiresAt <= DateTime.UtcNow)
            {
                otpData.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Mã OTP đã hết hạn."
                });
            }

            if (!VerifyOtp(request.Otp.Trim(), otpData.OtpHash))
            {
                return BadRequest(new
                {
                    message = "Mã OTP không chính xác."
                });
            }

            bool emailExists = await _context.guests.AnyAsync(x =>
                x.id != guestId &&
                x.email != null &&
                x.email.ToLower() == newEmail);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Email này đã được tài khoản khác sử dụng."
                });
            }

            guest.email = newEmail;
            otpData.IsUsed = true;
            otpData.IsVerified = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đổi email thành công.",
                email = guest.email
            });
        }
    }
}
