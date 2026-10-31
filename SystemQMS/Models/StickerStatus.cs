namespace SystemQMS.Models
{
    // Trạng thái phiếu (Sticker.status) - vòng đời phiếu do Toàn phụ trách.
    // Dùng hằng số thay vì gõ chuỗi để tránh lệch giữa các service.
    public static class StickerStatus
    {
        public const string Waiting = "Waiting";       // đang chờ gọi
        public const string Processing = "Processing"; // đang phục vụ tại quầy
        public const string Completed = "Completed";   // đã hoàn thành
        public const string Absent = "Absent";         // đã gọi nhưng khách vắng
        public const string Cancelled = "Cancelled";   // đã hủy
        public const string Expired = "Expired";       // hết hạn (quá hạn xử lý theo SRS)
    }

    // Giai đoạn xử lý chi tiết (Sticker.stage) - tách khỏi status để giữ được
    // trạng thái "đang chờ quầy khác phản hồi" mà phiếu vẫn là Waiting.
    public static class StickerStage
    {
        public const string Waiting = "Waiting";
        public const string Processing = "Processing";
        public const string TransferPending = "TransferPending"; // OPR-07: chờ quầy nhận đồng ý/từ chối
        public const string Accepted = "Accepted";               // quầy nhận đã đồng ý (kể cả auto)
        public const string Completed = "Completed";
        public const string Absent = "Absent";
        public const string Cancelled = "Cancelled";
    }

    // Kết quả hiển thị ở lịch sử quầy (OPR-13).
    public static class HistoryResult
    {
        public const string Completed = "Completed";
        public const string Absent = "Absent";
        public const string Cancelled = "Cancelled";
        public const string Expired = "Expired";
        public const string Transferred = "Transferred"; // phiếu đã được chuyển sang quầy khác
    }
}