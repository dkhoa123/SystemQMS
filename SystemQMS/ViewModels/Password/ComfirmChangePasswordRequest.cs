using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Password
{
    public class ComfirmChangePasswordRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ComfirmPassword { get; set; } = string.Empty;
    }
}
