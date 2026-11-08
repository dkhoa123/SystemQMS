using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Login
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        

        [MaxLength(100)]
        public string? NewEmail { get; set; }

        [MaxLength(30)]
        public string Purpose { get; set; } = "ForgotPassword";
    }
}

