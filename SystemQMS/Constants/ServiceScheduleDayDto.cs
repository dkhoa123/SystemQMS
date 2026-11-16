namespace SystemQMS.Constants
{
    public class ServiceScheduleDayDto
    {
        public bool enabled { get; set; }
        public string? startTime { get; set; }
        public string? endTime { get; set; }
        public List<string>? shiftIds { get; set; } = new List<string>();
    }
}
