using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.AuthDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthSvc _authSvc;
        public AuthController(IAuthSvc authSvc)
        {
            _authSvc = authSvc;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto users)
        {
            var result = _authSvc.Register(users);
            return Ok(result);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto users)
        {
            var result = _authSvc.Login(users);
            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Logout successful"
            });
        }

        // 1. Gửi OTP// Gửi OTP
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _authSvc.ForgotPasswordAsync(dto);

                return Ok(new
                {
                    message = "If the email exists, an OTP will be sent."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Xác thực OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(
            [FromBody] VerifyOtpDto dto)
        {
            bool result = await _authSvc.VerifyOtpAsync(dto);

            if (!result)
                return BadRequest(new { message = "OTP is invalid or expired." });

            return Ok(new { message = "OTP verified successfully." });
        }

        // Đổi mật khẩu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            try
            {
                bool result = await _authSvc.ResetPasswordAsync(dto);

                if (!result)
                    return BadRequest(new { message = "Please verify OTP first." });

                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
