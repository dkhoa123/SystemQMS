namespace SystemQMS.ViewModels.DevicesDto
{
    public class UpdateVolumnDeviceDto
    {
        public List<string> deviceIds { get; set; } = new List<string>();
        public int volume { get; set; } = 50;
    }
}
