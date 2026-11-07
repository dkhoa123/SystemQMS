using SystemQMS.ViewModels.DspDto;

namespace SystemQMS.Services.Interface
{
    public interface IDspSvc
    {
        Task<DisplayBoardPageResponseDto> GetDisplayBoardPageAsync(int page, bool showWaitlist, int unitId);
    }
}