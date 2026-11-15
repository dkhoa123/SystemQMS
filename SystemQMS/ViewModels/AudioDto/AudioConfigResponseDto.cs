namespace SystemQMS.ViewModels.AudioDto
{
    public class AudioConfigResponseDto
    {
        public int TemplateId { get; set; }
        public double ReadingSpeed { get; set; }
    }

    public class AudioConfigUpdateDto
    {
        public int TemplateId { get; set; }
        public double ReadingSpeed { get; set; } = 1.0;
    }

    public class AudioPreviewDto
    {
        public string SampleTicketNumber { get; set; } = string.Empty;
        public string SampleCounterName { get; set; } = string.Empty;
        public int TemplateId { get; set; } = 1;
        public double ReadingSpeed { get; set; } = 1.0;
    }

    public class AudioTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class AudioCallEventDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public List<string> AudioFiles { get; set; } = new();
        public double ReadingSpeed { get; set; } = 1.0;
        public int RepeatCount { get; set; } = 1;
    }
}