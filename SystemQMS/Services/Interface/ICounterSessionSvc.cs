using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Services.Interface
{
    public interface ICounterSessionSvc
    {
        // OPR-01: Quản lý phiên làm việc của cán bộ tại quầy
        Task<(bool Success, string Message, CounterSessionResponseDto? Data)> OpenSessionAsync(int counterId, OpenSessionRequestDto dto);
        Task<(bool Success, string Message)> CloseSessionAsync(int counterId, int userId);
        Task<CounterSessionResponseDto?> GetCurrentSessionAsync(int counterId);

        // OPR-10: Đóng/Mở trạng thái phục vụ quầy (Chặn nếu có Pending chuyển quầy, không sửa isActive)
        Task<(bool Success, string Message)> CloseCounterAsync(int counterId);
        Task<(bool Success, string Message)> OpenCounterAsync(int counterId);
    }
}