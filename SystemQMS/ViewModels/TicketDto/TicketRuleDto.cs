namespace SystemQMS.ViewModels.TicketDto
{
    public class TicketRuleDto
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public int StartNumber { get; set; }

        public int EndNumber { get; set; }

        public int CurrentNumber { get; set; }

        public bool ResetDaily { get; set; }
    }
}
