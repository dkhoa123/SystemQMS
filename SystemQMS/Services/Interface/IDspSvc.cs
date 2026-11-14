using SystemQMS.ViewModels.DspDto;

namespace SystemQMS.Services.Interface
{
    public interface IDspSvc
    {
        Task<DisplayBoardPageResponseDto> GetDisplayBoardPageAsync(int page, bool showWaitlist, int unitId);
        Task<DisplayCounterStatusResponseDto> GetDisplayCounterStatusAsync(int unitId);
        Task<CounterScreenStateResponseDto?> GetCounterScreenStateAsync(int counterId, bool showWaitlist);
        Task<CounterMultiCallDisplayResponseDto?> GetCounterMultiCallDisplayAsync(int counterId, int waitingLimit);
        Task<DisplayMaintenanceResponseDto> ReportDisplayErrorAndSwitchMaintenanceAsync(string deviceId, DisplayErrorRequestDto error);
    }
}