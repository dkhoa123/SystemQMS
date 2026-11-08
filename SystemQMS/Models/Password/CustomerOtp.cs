using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models.Password
{
    public class CustomerOtp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OtpHash { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; }
    }
}