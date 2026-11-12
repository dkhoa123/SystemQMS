using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Services.Interface
{
    public interface IEvaluationSvc
    {
        // RTG-01 + RTG-03: danh sách, cố định 10 dòng/trang, mới nhất trước
        Task<EvaluationListResponse> GetEvaluationsAsync(
            int unitId, int page, string? keyword, List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);

        // RTG-02: chi tiết 1 đánh 
        Task<EvaluationDetailDto> GetEvaluationAsync(int id, CancellationToken ct = default);

        // RTG-04: xuất Excel toàn bộ dòng khớp bộ lọc (không phân trang)
        Task<byte[]> ExportEvaluationsAsync(
            int unitId, string? keyword, List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);

        // RTG-06: tra cứu theo STT để popup tự điền (STT không tồn tại vẫn trả 200)
        Task<EvaluationLookupDto> LookupAsync(string sttId, int unitId, CancellationToken ct = default);

        // RTG-06/07: gửi đánh giá
        Task<EvaluationDetailDto> SubmitAsync(EvaluationSubmitDto dto, CancellationToken ct = default);

        // RTG-08: dữ liệu màn hình cho thiết bị đánh giá gắn tại quầy.
        // Truyền counterId, hoặc deviceId (thiết bị tự resolve ra quầy được gán cho nó).
        Task<RatingScreenDto> GetDeviceScreenAsync(int? counterId, string? deviceId, CancellationToken ct = default);
    }
}
