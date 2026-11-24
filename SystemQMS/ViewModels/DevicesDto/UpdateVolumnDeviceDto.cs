namespace SystemQMS.ViewModels.DevicesDto
{
    public class UpdateVolumnDeviceDto
    {
        public List<int> deviceIds { get; set; } = new List<int>();
        public int volume { get; set; } = 50;
    }
}
