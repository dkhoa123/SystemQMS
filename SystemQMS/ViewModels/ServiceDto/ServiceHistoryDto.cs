namespace SystemQMS.ViewModels.ServiceDto
{
    public class ServiceHistoryDto
    {
        public string id { get; set; }
        public string serviceCode { get; set; } = string.Empty;
        public string ticketNumber { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string performedBy { get; set; }
        public string usedAt { get; set; }
        public string counterName { get; set; }
    }
}
