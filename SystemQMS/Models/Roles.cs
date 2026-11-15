using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models
{
    public class Roles
    {
        [Key]
        public int roleId { get; set; }
        [MaxLength(100)]
        public string? roleName { get; set; }
        [MaxLength(255)]
        public string? description { get; set; }

        // Lưu danh sách mã quyền dưới dạng chuỗi
        public string? permissions { get; set; }

        public ICollection<Users> Users { get; set; } = new List<Users>();
    }
}
