using SystemQMS.ViewModels.AudioDto;
using SystemQMS.ViewModels.DspDto;

namespace SystemQMS.Services.Interface
{
    public interface IDspSvc
    {
        Task<AudioCallEventDto?> BuildAudioCallEventAsync(int counterId, string ticketNumber, int repeatCount);
        Task<AudioCallEventDto?> TriggerCallAudioAsync(int unitId, int counterId, string ticketNumber, int repeatCount = 1);
        Task<DisplayBoardPageResponseDto> GetDisplayBoardPageAsync(int page, bool showWaitlist, int unitId);
        Task<DisplayCounterStatusResponseDto> GetDisplayCounterStatusAsync(int unitId);
        Task<CounterScreenStateResponseDto?> GetCounterScreenStateAsync(int counterId, bool showWaitlist);
        Task<CounterMultiCallDisplayResponseDto?> GetCounterMultiCallDisplayAsync(int counterId, int waitingLimit);
        Task<DisplayMaintenanceResponseDto> ReportDisplayErrorAndSwitchMaintenanceAsync(string deviceId, DisplayErrorRequestDto error);
    }
}