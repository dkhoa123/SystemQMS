using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.RoleDto
{
    public class CreateRoleRequestDto
    {
        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên vai trò tối đa 100 ký tự")]
        public string roleName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? description { get; set; }

        public List<string> permissions { get; set; } = new();
    }
}