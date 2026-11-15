namespace SystemQMS.ViewModels.RoleDto
{
    public class PermissionGroupDto
    {
        public string groupCode { get; set; } = string.Empty;
        public string groupName { get; set; } = string.Empty;
        public List<PermissionFunctionDto> functions { get; set; } = new();
    }
}