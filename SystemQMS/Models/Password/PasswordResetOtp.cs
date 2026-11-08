using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models.Password
{
    public class PasswordResetOtp
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
      

        [Required]
        public string OtpHash { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? NewEmail { get; set; }

        [MaxLength(30)]
        public string Purpose { get; set; } = "ForgotPassword";

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
