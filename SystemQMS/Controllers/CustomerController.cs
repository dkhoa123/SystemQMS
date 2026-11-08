
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Models.Password;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CustomerDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IEmailService _emailService;

        // Hash OTP và mật khẩu
        private readonly PasswordHasher<CustomerOtp> _otpHasher = new();
        private readonly PasswordHasher<Guest> _guestHasher = new();

        public CustomerController(
            MyDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/Customer/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Email và mật khẩu không được để trống."
                });
            }

            string email = request.Email.Trim().ToLowerInvariant();

            if (request.Password.Length < 6)
            {
                return BadRequest(new
                {
                    message = "Mật khẩu phải có ít nhất 6 ký tự."
                });
            }

            // Kiểm tra email đã tồn tại trong Guest
            bool guestExists = await _context.guests
                .AnyAsync(x => x.email != null &&
                               x.email.ToLower() == email);

            if (guestExists)
            {
                return BadRequest(new
                {
                    message = "Email đã được sử dụng."
                });
            }

            // Kiểm tra email đã tồn tại trong Users
            bool userExists = await _context.users
                .AnyAsync(x => x.email != null &&
                               x.email.ToLower() == email);

            if (userExists)
            {
                return BadRequest(new
                {
                    message = "Email đã được sử dụng."
                });
            }

            // Xóa OTP đăng ký cũ chưa sử dụng
            var oldOtps = await _context.CustomerOtps
                .Where(x => x.email == email && !x.IsUsed)
                .ToListAsync();

            _context.CustomerOtps.RemoveRange(oldOtps);

            string otp = Random.Shared
                .Next(100000, 1000000)
                .ToString();

            var pending = new CustomerOtp
            {
                email = email,

                // Hash mật khẩu theo kiểu Guest
                PasswordHash = _guestHasher.HashPassword(
                    new Guest(),
                    request.Password),

                // Hash OTP
                OtpHash = _otpHasher.HashPassword(
                    new CustomerOtp(),
                    otp),

                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.CustomerOtps.Add(pending);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendOtpAsync(email, otp);

                return Ok(new
                {
                    message = "OTP đã được gửi đến email của bạn.",
                    expiresIn = "5 phút"
                });
            }
            catch (Exception ex)
            {
                _context.CustomerOtps.Remove(pending);
                await _context.SaveChangesAsync();

                Console.WriteLine("Register email error: " + ex);

                return StatusCode(500, new
                {
                    message = "Gửi OTP thất bại. Vui lòng thử lại."
                });
            }
        }

        // POST: api/Customer/verify-otp-cus
        [HttpPost("verify-otp-cus")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new
                {
                    message = "Email và OTP không được để trống."
                });
            }

            string email = request.Email.Trim().ToLowerInvariant();

            var pending = await _context.CustomerOtps
                .Where(x => x.email == email && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (pending == null)
            {
                return BadRequest(new
                {
                    message = "Không tìm thấy yêu cầu đăng ký."
                });
            }

            if (pending.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                pending.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "OTP đã hết hạn. Vui lòng đăng ký lại."
                });
            }

            // Kiểm tra OTP
            var otpResult = _otpHasher.VerifyHashedPassword(
                pending,
                pending.OtpHash,
                request.Otp.Trim());

            if (otpResult == PasswordVerificationResult.Failed)
            {
                return BadRequest(new
                {
                    message = "OTP không chính xác."
                });
            }

            // Kiểm tra email trước khi tạo Guest
            bool guestExists = await _context.guests
                .AnyAsync(x => x.email != null &&
                               x.email.ToLower() == email);

            bool userExists = await _context.users
                .AnyAsync(x => x.email != null &&
                               x.email.ToLower() == email);

            if (guestExists || userExists)
            {
                pending.IsUsed = true;
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Email đã được sử dụng."
                });
            }

            // Tạo tài khoản Guest
            var customer = new Guest
            {
                guestId = Guid.NewGuid().ToString(),
                email = pending.email,
                password = pending.PasswordHash,
                unitId = null,
                registerTime = DateTimeOffset.UtcNow,
                status = "Active"
            };

            _context.guests.Add(customer);

            // Đánh dấu OTP đã sử dụng
            pending.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xác minh thành công. Tài khoản Guest đã được tạo."
            });
        }
    }
}
