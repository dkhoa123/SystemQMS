namespace SystemQMS.Models.DTOs
{
    public class CentralScreenDataDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; }

        public string CurrentTicket { get; set; }

        public List<string> WaitingTickets { get; set; } = new List<string>();
    }
}