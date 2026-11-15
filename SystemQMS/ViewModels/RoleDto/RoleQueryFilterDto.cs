namespace SystemQMS.ViewModels.RoleDto
{
    public class RoleQueryFilterDto
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? keyword { get; set; }
    }
}