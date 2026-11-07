namespace SystemQMS.ViewModels.CountersDto
{
    public class TicketOperationResponseDto
    {
        public string sttId { get; set; } = string.Empty;
        public int serviceId { get; set; }
        public string serviceName { get; set; } = string.Empty;
        public int counterId { get; set; }
        public string counterName { get; set; } = string.Empty;
        public int? userId { get; set; }
        public string? staffName { get; set; }
        public string status { get; set; } = string.Empty;
        public string stage { get; set; } = string.Empty;
        public bool isPriority { get; set; }
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset? useTime { get; set; }
    }
}