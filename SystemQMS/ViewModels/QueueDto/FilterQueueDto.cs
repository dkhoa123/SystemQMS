namespace SystemQMS.ViewModels.QueueDto
{
    public class FilterQueueDto
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? keyword { get; set; }
        public string? status { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

    }
}
