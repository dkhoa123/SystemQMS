namespace SystemQMS.ViewModels.DspDto
{
    public class MainDisplayResponseDto
    {
        public DateTime CurrentTime { get; set; }
        public bool ShowWaitlist { get; set; }
        public List<MainDisplayCounterDto> Counters { get; set; } = new();
    }

    public class MainDisplayCounterDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public bool HasActiveTicket { get; set; }
        public List<string> CallingTickets { get; set; } = new();
        public List<string> WaitingTickets { get; set; } = new();
    }

    public class CounterDisplayResponseDto
    {
        public int CounterId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool ShowWaitlist { get; set; }
        public List<TicketDisplayDto> CallingTickets { get; set; } = new();
        public List<string> WaitingTickets { get; set; } = new();
    }

    public class TicketDisplayDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
    }

    public class ErrorLogRequestDto
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}