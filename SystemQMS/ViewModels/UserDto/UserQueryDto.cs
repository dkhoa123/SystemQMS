namespace SystemQMS.ViewModels.UserDto
{
    public class UserQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
    }
}