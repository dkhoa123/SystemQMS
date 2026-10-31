using SystemQMS.ViewModels.QueueDto;

namespace SystemQMS.Services.Interface
{
    // OPR-09 / OPR-12 / OPR-13 / OPR-14: điều phối hàng chờ tại quầy
    public interface IQueueSvc
    {
        // OPR-09: ngưng nhận phiếu mới
        Task<CounterStateResponse> PauseAcceptingAsync(int counterId, CancellationToken ct = default);

        // OPR-09: nhận phiếu lại (về Serving nếu còn phiếu đang xử lý, ngược lại Idle)
        Task<CounterStateResponse> ResumeAcceptingAsync(int counterId, CancellationToken ct = default);

        // OPR-09:endpoint bật/tắt 
        Task<CounterStateResponse> ToggleReceivingAsync(int counterId, CancellationToken ct = default);

        // OPR-14: đặt phiếu ưu tiên (được xếp lên đầu hàng chờ)
        Task<QueueTicketDto> SetPriorityAsync(string sttId, CancellationToken ct = default);

        // OPR-12: 3 nhóm phiếu waiting / processing / absent của quầy
        Task<CounterTicketsResponse> GetCounterTicketsAsync(int counterId, CancellationToken ct = default);

        // OPR-13: lịch sử quầy (phiếu kết thúc + phiếu đã chuyển đi), có phân trang
        Task<CounterHistoryResponse> GetCounterHistoryAsync(
            int counterId, int page = 1, int pageSize = 20,DateTimeOffset? from = null,DateTimeOffset? to = null, CancellationToken ct = default);
    }
}