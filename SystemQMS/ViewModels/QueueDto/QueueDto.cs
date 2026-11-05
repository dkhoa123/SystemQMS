namespace SystemQMS.ViewModels.QueueDto
{
    // Phiếu trong hàng chờ của quầy (OPR-09 / OPR-12)
    public class QueueTicketDto
    {
        public string sttId { get; set; } = "";
        public string status { get; set; } = "";
        public string stage { get; set; } = "";
        public bool isPriority { get; set; }
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset useTime { get; set; }
        public string? serviceName { get; set; }
        public string? handlerName { get; set; }
    }

    public class CounterTicketsResponse
    {
    public int counterId { get; set; }
    public string counterStatus { get; set; } = "";
    public List<QueueTicketDto> waiting { get; set; } = new();
    public List<QueueTicketDto> processing { get; set; } = new();
    public List<QueueTicketDto> absent { get; set; } = new();
    }

    // Trạng thái nhận phiếu của quầy (OPR-09)
    public class CounterStateResponse
    {
        public int counterId { get; set; }
        public string status { get; set; } = "";
        public bool isAccepting { get; set; }
        public int waitingCount { get; set; }
        public int processingCount { get; set; }
    }

    // Lịch sử quầy (OPR-13)
    public class HistoryItemResponse
    {
        public string sttId { get; set; } = "";
        public string result { get; set; } = ""; // Completed | Absent | Cancelled | Transferred
        public DateTimeOffset time { get; set; }
        public string? serviceName { get; set; } 
        public string? handlerName { get; set; } // người thực hiện
        public string? counterName { get; set; } // quầy đích, chỉ có khi result = Transferred
        public bool isPriority { get; set; }
    }

    // Lịch sử quầy có phân trang (sort giảm dần theo thời gian rồi mới cắt trang)
    public class CounterHistoryResponse
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
        public List<HistoryItemResponse> items { get; set; } = new();
    }
}
