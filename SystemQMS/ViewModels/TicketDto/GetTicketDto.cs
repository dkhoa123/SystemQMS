namespace SystemQMS.ViewModels.TicketDto
{
    public class GetTicketDto
    {
        public string? id { get; set; }
        public string? ticketNumber { get; set; }
        public string? serviceName { get; set; }
        public string? issuedTime { get; set; }
        public string? usedTime { get; set; }
        public string? status { get; set; }
        public string? staffName { get; set; }
        public string? counterName { get; set; }
    }
}
