namespace SystemQMS.ViewModels.DspDto
{
    public class CounterScreenStateResponseDto
    {
        public DateTime CurrentTime { get; set; }
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public bool ShowWaitlist { get; set; }
        public string ScreenState { get; set; } = "Active";
        public string? ScreenMessage { get; set; }
        public bool IsGrayScreen { get; set; }
        public CounterCalledTicketDto? CalledTicket { get; set; }
        public List<CounterWaitingTicketDto> WaitingTickets { get; set; } = new();
    }

    public class CounterMultiCallDisplayResponseDto
    {
        public DateTime CurrentTime { get; set; }
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public List<CounterCalledTicketDto> CallingTickets { get; set; } = new();
        public List<CounterWaitingTicketDto> WaitingTickets { get; set; } = new();
    }

    public class CounterCalledTicketDto
    {
        public string SttId { get; set; } = string.Empty;
        public int? TicketNumber { get; set; }
    }

    public class CounterWaitingTicketDto
    {
        public string SttId { get; set; } = string.Empty;
        public int? TicketNumber { get; set; }
    }

    public class DisplayErrorRequestDto
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class DisplayMaintenanceResponseDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Mode { get; set; } = "MAINTENANCE";
        public string Message { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class DisplayCounterStatusResponseDto
    {
        public DateTime CurrentTime { get; set; }
        public List<DisplayCounterStatusDto> Counters { get; set; } = new();
    }

    public class DisplayCounterStatusDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public string CounterStatus { get; set; } = string.Empty;
        public bool IsServing { get; set; }
        public bool IsDimmed { get; set; }
    }

    public class DisplayBoardPageResponseDto
    {
        public DateTime CurrentTime { get; set; }
        public bool ShowWaitlist { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRows { get; set; }
        public int AutoSwitchSeconds { get; set; }
        public int NextPage { get; set; }
        public List<DisplayBoardRowDto> Rows { get; set; } = new();
    }

    public class DisplayBoardRowDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public string CalledSttId { get; set; } = string.Empty;
        public int? CalledTicketNumber { get; set; }
        public string? WaitingSttId { get; set; }
        public int? WaitingTicketNumber { get; set; }
    }
}