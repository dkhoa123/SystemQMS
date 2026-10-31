using SystemQMS.ViewModels.TransfersDto;

namespace SystemQMS.Services.Interface
{
    // OPR-07: chuyển phiếu giữa các quầy
    public interface ITransferSvc
    {
        // Quầy gửi tạo yêu cầu chuyển phiếu -> CounterChange Pending + stage TransferPending
        Task<List<TransferResponse>> CreateTransferAsync(
            int counterId, TransferRequest request, CancellationToken ct = default);

        // Quầy nhận phản hồi: Accept | Reject
        Task<TransferResponse> ActionAsync(int transferId, string action, CancellationToken ct = default);

        // Danh sách yêu cầu đang chờ quầy này tiếp nhận (mới nhất trước)
        Task<List<TransferResponse>> GetPendingTransfersAsync(int counterId, CancellationToken ct = default);
    }
}
