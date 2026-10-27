using SystemQMS.ViewModels.AudioDto;

namespace SystemQMS.Services
{
    public interface IConfigSvc
    {
        Task<DisplayConfigResponseDto> GetDisplayConfigAsync(int unitId);
        Task<DisplayConfigResponseDto> UpdateDisplayConfigAsync(int unitId, DisplayConfigUpdateDto dto);
        Task ResetDisplayFieldAsync(int unitId, string fieldName);
        Task UpdateAudioConfigAsync(int unitId, int templateId);
    }
}
