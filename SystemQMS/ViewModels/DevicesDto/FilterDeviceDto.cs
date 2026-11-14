namespace SystemQMS.ViewModels.DevicesDto
{
    public class FilterDeviceDto
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string? keyword { get; set; }
        public string? availability { get; set; }
        public string? connectionStatus { get; set; }
    }
}
