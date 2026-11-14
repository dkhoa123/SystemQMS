using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models
{
    public class Roles
    {
        [Key]
        public int RoleId { get; set; }
        [MaxLength(100)]
        public string? RoleName { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }

        public ICollection<Users> Users { get; set; } = new List<Users>();
    }
}
