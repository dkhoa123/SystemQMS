namespace SystemQMS.ViewModels.DspDto
{
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