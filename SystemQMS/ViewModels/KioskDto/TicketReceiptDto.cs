namespace SystemQMS.ViewModels.KioskDto
{
    public class TicketReceiptDto
    {
        public string ticketNumber { get; set; }
        public string serviceName { get; set; }
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset expiryTime { get; set; }
        public int? waitMinute { get; set; }

        public string QrCodeData { get; set; }

    }
}
