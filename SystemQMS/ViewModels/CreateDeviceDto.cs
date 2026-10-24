namespace SystemQMS.ViewModels
{
    public class CreateDeviceDto
    {
        public string deviceId { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public List<string>? serviceIds { get; set; } = new List<string>();
    }
}
