namespace SystemQMS.ViewModels.PublicService
{
    public class PublicServiceResponse
    {
        public int id { get; set; }
        public string serviceId { get; set; } = string.Empty;
        public string serviceName { get; set; } = string.Empty;
        public string? description { get; set; }
        public byte[]? image { get; set; }
        public string sharingLink { get; set; } = string.Empty;
        public DateTimeOffset averageTime { get; set; }
    }
}
