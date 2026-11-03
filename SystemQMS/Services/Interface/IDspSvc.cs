using SystemQMS.ViewModels.DspDto;

namespace SystemQMS.Services
{
    public interface IDspSvc
    {
        Task<MainDisplayResponseDto> GetMainDisplayStateAsync();

        Task<CounterDisplayResponseDto?> GetCounterDisplayStateAsync(int counterId);

        Task LogDeviceErrorAsync(string deviceId, ErrorLogRequestDto error);
    }
}