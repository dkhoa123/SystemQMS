using SystemQMS.ViewModels.AudioDto;

namespace SystemQMS.Services.Interface
{
    public interface IConfigSvc
    {
        Task<DisplayConfigResponseDto> GetDisplayConfigAsync(int unitId);
        Task<DisplayConfigResponseDto> UpdateDisplayConfigAsync(int unitId, DisplayConfigUpdateDto dto);
        Task ResetDisplayFieldAsync(int unitId, string fieldName);
        Task<AudioConfigResponseDto> GetAudioConfigAsync(int unitId);
        Task UpdateAudioConfigAsync(int unitId, AudioConfigUpdateDto dto);
    }
}
