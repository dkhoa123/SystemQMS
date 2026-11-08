
using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models.Password
{
    public class RevokedToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
