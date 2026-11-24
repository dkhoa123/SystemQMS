namespace SystemQMS.ViewModels.ServiceDto
{
    public class FilterServiceDto
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;

        public string? keyword { get; set; }

        public string? status { get; set; }

        public DateTime? fromDate { get; set; }

        public DateTime? toDate { get; set; }
    }
}
