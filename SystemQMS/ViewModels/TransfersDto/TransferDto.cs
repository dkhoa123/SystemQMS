namespace SystemQMS.ViewModels.TransfersDto
{
    // OPR-07: yêu cầu chuyển 1 hoặc nhiều phiếu sang quầy khác
    public class TransferRequest
    {
        public List<string> sttIds { get; set; } = new();
        public int toCounterId { get; set; }
        // Đánh dấu phiếu ưu tiên ngay khi chuyển (tùy chọn)
        public bool? isPriority { get; set; }
    }

    // OPR-07: quầy nhận phản hồi
    public class TransferActionRequest
    {
        // "Accept" | "Reject"
        public string action { get; set; } = "";
    }

    public class TransferResponse
    {
        public int id { get; set; }
        public string sttId { get; set; } = "";
        public int counterIdSender { get; set; }
        public int counterIdReceive { get; set; }
        public string status { get; set; } = "";
        public DateTimeOffset createTime { get; set; }
    }
}
