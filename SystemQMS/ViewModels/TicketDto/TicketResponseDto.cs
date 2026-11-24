namespace SystemQMS.ViewModels.TicketDto
{
    public class TicketResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public string TicketNumber { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public DateTimeOffset IssueTime { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
