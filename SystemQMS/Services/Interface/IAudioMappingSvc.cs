namespace SystemQMS.Services.Interface
{
    public interface IAudioMappingSvc
    {
        List<string> BuildAudioSequence(string ticketNumber, string counterName, int templateId);
    }
}
