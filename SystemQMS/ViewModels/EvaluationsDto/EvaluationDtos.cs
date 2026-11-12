namespace SystemQMS.ViewModels.EvaluationsDto
{
    // Lấy Một dòng danh sách đánh giá (RTG-01
    public class EvaluationListItemDto
    {
        public int id { get; set; }
        public string sttId { get; set; } = "";
        public int counterId { get; set; }
        public string? counterName { get; set; }
        public string? serviceName { get; set; }
        public DateTimeOffset evaluationTime { get; set; }
        public string content { get; set; } = "";
        public string? opinion { get; set; }
    }

    // Danh sách có phân trang (RTG-01 cố định 10 dòng/trang)
    public class EvaluationListResponse
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public List<EvaluationListItemDto> items { get; set; } = new();
    }

    // Chi tiết 1 đánh giá (RTG-02)
    public class EvaluationDetailDto
    {
        public int id { get; set; }
        public string sttId { get; set; } = "";
        public int unitId { get; set; }
        public int counterId { get; set; }
        public string? counterName { get; set; }
        public int serviceId { get; set; }
        public string? serviceName { get; set; }
        public string? templateId { get; set; }
        public int timeMark { get; set; }
        public int attitudeMark { get; set; }
        public int materialMark { get; set; }
        public int satisfedMark { get; set; }
        public string content { get; set; } = "";
        public string? opinion { get; set; }
        public string? guestPhone { get; set; }
        public string? guestName { get; set; }
        public DateTimeOffset evaluationTime { get; set; }
    }

    // Kết quả tra cứu STT cho popup Kiosk (RTG-06) — STT không tồn tại thì KHÔNG 404,
    // trả counterId/serviceId = null để frontend biết đây là ca RTG-07
    public class EvaluationLookupDto
    {
        public string sttId { get; set; } = "";
        public bool found { get; set; }
        public int? counterId { get; set; }
        public string? counterName { get; set; }
        public int? serviceId { get; set; }
        public string? serviceName { get; set; }
        public string? handlerName { get; set; }
        public string? templateId { get; set; }
        public string? contentQuestion { get; set; }
    }

    // Màn hình thiết bị đánh giá theo quầy (RTG-08, kèm dịch vụ của quầy = RTG-10).
    // Endpoint này cũng là payload của event realtime OnRatingContextChanged (RTG-11).
    public class RatingScreenDto
    {
        public int counterId { get; set; }
        public string? counterName { get; set; }
        public string counterStatus { get; set; } = "";

        // Phiếu thiết bị sẽ đánh giá: phiếu đang Processing tại quầy; nếu chưa có ai
        // đang xử lý thì lấy phiếu Completed gần nhất trong ngày (khách vừa được phục vụ
        // xong là lúc đánh giá tự nhiên nhất). Không có cả hai -> null và canRate = false.
        public string? currentSttId { get; set; }
        public string? currentTicketStatus { get; set; }
        public bool canRate { get; set; }
        public int? handlerUserId { get; set; }
        public string? handlerName { get; set; }
        public string? handlerAvatarBase64 { get; set; }
        public List<RatingScreenServiceDto> services { get; set; } = new();
        public string? templateId { get; set; }
        public string? contentQuestion { get; set; }
    }

    public class RatingScreenServiceDto
    {
        public int serviceId { get; set; }
        public string? serviceName { get; set; }
    }

    // Gửi đánh giá từ Kiosk (RTG-06/07)
    // counterId/serviceId chỉ dùng cho đường STT chưa tồn tại (khi đã bỏ FK);
    // STT đã tồn tại thì service lấy từ chính phiếu, không tin request.
    public class EvaluationSubmitDto
    {
        public string sttId { get; set; } = "";
        public int unitId { get; set; }
        public int? counterId { get; set; }
        public int? serviceId { get; set; }
        public string? templateId { get; set; }
        public int timeMark { get; set; }
        public int attitudeMark { get; set; }
        public int materialMark { get; set; }
        public int satisfedMark { get; set; }
        public string? guestPhone { get; set; }
        public string? guestName { get; set; }
        public string? opinion { get; set; }
    }
}
