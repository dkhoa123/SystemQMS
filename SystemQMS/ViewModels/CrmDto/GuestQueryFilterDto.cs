namespace SystemQMS.ViewModels.CrmDto
{
    public class GuestQueryFilterDto
    {
        public int? unitId { get; set; }
        public string? keyword { get; set; }
        public string? status { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}