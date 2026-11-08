using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.Login;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ITokenRevocationService _tokenService;
        private readonly PasswordHasher<Users> _userHasher;
        private readonly PasswordHasher<Guest> _guestHasher;

        public AuthController(
            MyDbContext context,
            IJwtService jwtService,
            ITokenRevocationService tokenService)
        {
            _context = context;
            _jwtService = jwtService;
            _tokenService = tokenService;

            _userHasher = new PasswordHasher<Users>();
            _guestHasher = new PasswordHasher<Guest>();
        }

        // POST: api/Auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Vui lòng nhập đầy đủ tài khoản và mật khẩu."
                });
            }

            string username = request.Username.Trim();

            // 1. Thử đăng nhập nhân viên từ Users
            var user = await _context.users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.username == username);

            if (user != null)
            {
                if (string.IsNullOrWhiteSpace(user.password))
                    return Unauthorized(new
                    {
                        message = "Tên đăng nhập hoặc mật khẩu không đúng."
                    });

                if (!user.isActive)
                    return Unauthorized(new
                    {
                        message = "Tài khoản đã bị khóa hoặc chưa kích hoạt."
                    });

                if (!user.isVerified)
                    return Unauthorized(new
                    {
                        message = "Tài khoản chưa được xác minh."
                    });

                var result = _userHasher.VerifyHashedPassword(
                    user,
                    user.password,
                    request.Password);

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized(new
                    {
                        message = "Tên đăng nhập hoặc mật khẩu không đúng."
                    });

                string roleName = user.Role?.roleName ?? "User";

                string token = _jwtService.GenerateToken(
                    user.userId,
                    user.username!,
                    roleName);

                return CreateLoginResponse(
                    user.userId,
                    user.username!,
                    roleName,
                    token);
            }

            // 2. Nếu không có trong Users, thử đăng nhập Guest bằng email
            string email = username.ToLowerInvariant();

            var guest = await _context.guests
                .FirstOrDefaultAsync(x =>
                    x.email != null &&
                    x.email.ToLower() == email);

            if (guest == null || string.IsNullOrWhiteSpace(guest.password))
            {
                return Unauthorized(new
                {
                    message = "Tài khoản hoặc mật khẩu không đúng."
                });
            }

            var guestResult = _guestHasher.VerifyHashedPassword(
                guest,
                guest.password,
                request.Password);

            if (guestResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Tài khoản hoặc mật khẩu không đúng."
                });
            }

            // 3. Tạo JWT cho khách hàng
            string guestToken = _jwtService.GenerateToken(
                guest.id,
                guest.email!,
                "Guest");

            return CreateLoginResponse(
                guest.id,
                guest.email!,
                "Guest",
                guestToken);
        }

        // Tạo response đăng nhập
        private IActionResult CreateLoginResponse(
            int id,
            string username,
            string role,
            string token)
        {
            double duration = double.TryParse(
                HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:Duration"],
                out double minutes) ? minutes : 30000;

            var response = new LoginResponse
            {
                UserId = id,
                Username = username,
                Role = role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(duration)
            };

            return Ok(new
            {
                message = "Đăng nhập thành công.",
                data = response
            });
        }

        // POST: api/Auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    message = "Không tìm thấy JWT token."
                });
            }

            string token = authHeader.Substring(7).Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new
                {
                    message = "JWT token không hợp lệ."
                });
            }

            await _tokenService.RevokeTokenAsync(token);

            return Ok(new
            {
                message = "Đăng xuất thành công!"
            });
        }






    }
}