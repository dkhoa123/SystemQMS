namespace SystemQMS.ViewModels.RoleDto
{
    public class RolePageResultDto<T>
    {
        public List<T> items { get; set; } = new();
        public int page { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
    }
}