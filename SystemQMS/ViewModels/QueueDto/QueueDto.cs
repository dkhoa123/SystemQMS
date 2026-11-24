namespace SystemQMS.ViewModels.QueueDto
{
    // Phiếu trong hàng chờ của quầy (OPR-09 / OPR-12)
    public class QueueTicketDto
    {
        public string id { get; set; }
        public string number { get; set; }
        public string issuedAt { get; set; }
        public string? serviceName { get; set; }
        public string? status { get; set; }
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
        public int issued { get; set; }
        public int waiting { get; set; }
        public int used { get; set; }
        public int skipped { get; set; }
        public int cancelled { get; set; }
    }

    // Lịch sử quầy (OPR-13)
    public class HistoryItemResponse
    {
        public string? id { get; set; }
        public string? number { get; set; }
        public string? serviceName { get; set; }
        public string? staffName { get; set; }
        public string? result { get; set; }
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
