using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels
{
    public class PageResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
    public class RoleDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateUpdateRoleDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên vai trò")]
        [MaxLength(100, ErrorMessage = "Tên vai trò tối đa 100 ký tự")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? Description { get; set; }

        public List<string> Permissions { get; set; } = new();
    }

    public class PermissionFunctionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class PermissionGroupDto
    {
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public List<PermissionFunctionDto> Functions { get; set; } = new();
    }
}