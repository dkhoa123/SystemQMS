namespace SystemQMS.Services
{
    public interface IAudioMappingSvc
    {
        List<string> BuildAudioSequence(string ticketNumber, string counterName, int templateId);
    }
}
