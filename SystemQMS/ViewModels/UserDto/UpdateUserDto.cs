namespace SystemQMS.ViewModels.UserDto
{
    public class UpdateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public string? Status { get; set; } = "ACTIVE";
    }
}