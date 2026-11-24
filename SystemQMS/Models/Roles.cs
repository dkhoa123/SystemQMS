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

        // thêm trường mới để lưu danh sách mã quyền dạng Json
        public string? Permissions { get; set; }
        public ICollection<Users> Users { get; set; } = new List<Users>();
    }
}
