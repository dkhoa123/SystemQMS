namespace SystemQMS.ViewModels
{
    public class UpdateDeviceDto
    {
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public string statusActive { get; set; }
        public string statusConnect { get; set; }
        public string ipAddress { get; set; }
        public int volume { get; set; }
        public bool show { get; set; }
        public List<string>? serviceIdsRemove { get; set; } = new List<string>();
        
    }
}
