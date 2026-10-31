using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Services.Interface
{
    public interface ICounterTicketSvc
    {
        // OPR-02: Gọi số kế tiếp (Ưu tiên isPriority, né TransferPending, kiểm tra HRS-01)
        Task<(bool Success, string Message, TicketOperationResponseDto? Data)> CallNextTicketAsync(int counterId, int userId);

        // OPR-03 & OPR-08: Gọi nhiều số cùng lúc từ cấu hình appsettings.json
        Task<(bool Success, string Message, List<TicketOperationResponseDto> Data)> CallMultipleTicketsAsync(int counterId, int userId);

        // OPR-04: Hoàn thành lượt phục vụ (Chặn vé TransferPending)
        Task<(bool Success, string Message, TicketOperationResponseDto? Data)> CompleteTicketAsync(int counterId, string sttId, int userId);

        // OPR-05: Bỏ qua / Khách vắng mặt (Chặn vé TransferPending)
        Task<(bool Success, string Message, TicketOperationResponseDto? Data)> SkipTicketAsync(int counterId, string sttId, int userId, SkipTicketRequestDto? dto);

        // OPR-06: Gọi lại vé đang xử lý hoặc vừa bị bỏ qua
        Task<(bool Success, string Message, TicketOperationResponseDto? Data)> RecallTicketAsync(int counterId, string sttId, int userId);
    }
}