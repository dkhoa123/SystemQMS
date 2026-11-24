using System.Net;

namespace SystemQMS.ViewModels.DevicesDto
{
    public class DetailDevicesDto
    {
        public string deviceCode { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public string? password { get; set; }
        public string connectionStatus { get; set; }
        public string? ipAddress { get; set; }
        public int volume { get; set; }
        public List<string> serviceIds { get; set; } = new List<string>();

    }
}
