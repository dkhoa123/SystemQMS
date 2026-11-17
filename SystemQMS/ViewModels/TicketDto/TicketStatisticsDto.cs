namespace SystemQMS.ViewModels.TicketDto
{
    public class TicketStatisticsDto
    {
        public int totalIssued { get; set; }
        public int waiting { get; set; }
        public int isUsed { get; set; }
        public int skipped { get; set; }
    }
}
