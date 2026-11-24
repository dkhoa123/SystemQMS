namespace SystemQMS.ViewModels.DevicesDto
{
    public class CreateDeviceDto
    {
        public string deviceCode { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public string availability { get; set; }
        public List<string>? serviceIds { get; set; } = new List<string>();
        public string? description { get; set; }
        public string templateId { get; set; }
    }
}
