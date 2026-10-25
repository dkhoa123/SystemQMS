using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Users
    {
        [Key]
        public int userId { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string? username { get; set; }
        [MaxLength(100)]
        public string? password { get; set; }
        [MaxLength(100)]
        public string? email { get; set; }
        [MaxLength(10)]
        public string? numberPhone { get; set; }
        public int roleId { get; set; }
        public byte[]? avatar { get; set; }
        public bool isActive { get; set; }
        public int counterId { get; set; }

        [ForeignKey("roleId")]
        public virtual Roles? Role { get; set; }
        [ForeignKey("counterId")]
        public virtual Counter? Counter { get; set; }
        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<Diary>? Diaries { get; set; } = new List<Diary>();
        public ICollection<Sticker>? Stickers { get; set; } = new List<Sticker>();

    }
}
