using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public int UnitId { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Username { get; set; }
        [MaxLength(100)]
        public string? Password { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(10)]
        public string? NumberPhone { get; set; }
        public int RoleId { get; set; }
        public byte[]? Avatar { get; set; }
        public bool IsActive { get; set; }
        public int CounterId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Roles? Role { get; set; }
        [ForeignKey("CounterId")]
        public virtual Counter? Counter { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<Diary>? Diaries { get; set; } = new List<Diary>();
        public ICollection<Ticket>? Tickets { get; set; } = new List<Ticket>();

    }
}
