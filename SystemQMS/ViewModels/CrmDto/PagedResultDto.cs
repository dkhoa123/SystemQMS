namespace SystemQMS.ViewModels.CrmDto
{
    public class PagedResultDto<T>
    {
        public int totalItems { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalPages => (int)Math.Ceiling((double)totalItems / (pageSize > 0 ? pageSize : 10));
        public List<T> items { get; set; } = new List<T>();
    }
}