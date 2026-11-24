namespace SystemQMS.ViewModels.CountersDto
{
    public class FilterCounterDto
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? keyword { get; set; }
        public string? serviceName { get; set; }
        public string? use { get; set; }
    }
}
