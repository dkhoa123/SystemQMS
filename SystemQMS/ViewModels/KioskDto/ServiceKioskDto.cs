namespace SystemQMS.ViewModels.KioskDto
{
    public class ServiceKioskDto
    {
        public string serviceId { get; set; }
        public string serviceName { get; set; }
        public string statusActive { get; set; }
        public bool isDisabled { get; set; }
        public bool isBlurred { get; set; }
        public string LastSTT { get; set; }
        public double lastMinutes { get; set; }
    }
}
