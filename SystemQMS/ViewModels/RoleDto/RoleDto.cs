namespace SystemQMS.ViewModels.RoleDto
{
    public class RoleDto
    {
        public string roleId { get; set; } = string.Empty;
        public string roleName { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int userCount { get; set; }
        public List<string> permissions { get; set; } = new();
    }
}