namespace SystemQMS.ViewModels.DevicesDto
{
    public class GetDevicesDto
    {
        public int deviceId { get; set; }
        public string deviceCode { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public string availability { get; set; }
        public string connectionStatus { get; set; }
        public string? ipAddress { get; set; }
        public int volume { get; set; }
        public bool showQueue { get; set; }
        public string? kioskTemplateId { get; set; }
        public List<string>? serviceIds { get; set; } = new List<string>();
        public List<string>? counterIds { get; set; } = new List<string>();
        public string? description { get; set; }
    }
}
