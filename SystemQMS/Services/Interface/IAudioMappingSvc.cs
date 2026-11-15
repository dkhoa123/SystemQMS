using SystemQMS.ViewModels.AudioDto;

namespace SystemQMS.Services.Interface
{
    public interface IAudioMappingSvc
    {
        List<string> BuildAudioSequence(string ticketNumber, string counterName, int templateId);
        Task<AudioCallEventDto> BuildAudioCallEventAsync(int unitId, int counterId, string counterName, string ticketNumber, int repeatCount = 1);
    }
}
