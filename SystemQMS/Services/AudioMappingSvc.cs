namespace SystemQMS.Services
{
    public class AudioMappingService : IAudioMappingSvc
    {
        private readonly string _audioBaseUrl = "/AudioConfigs/";

        public List<string> BuildAudioSequence(string ticketNumber, string counterName, int templateId)
        {
            var audioFiles = new List<string>();

            if (!string.IsNullOrEmpty(ticketNumber))
            {
                foreach (char c in ticketNumber.ToUpper())
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        audioFiles.Add($"{_audioBaseUrl}{c}-default.wav");
                    }
                }
            }

            switch (templateId)
            {
                default:
                    audioFiles.Add($"{_audioBaseUrl}come-to.wav");
                    break;
            }

            if (!string.IsNullOrEmpty(counterName))
            {
                foreach (char c in counterName.ToUpper())
                {
                    if (char.IsLetterOrDigit(c))
                    {
                        audioFiles.Add($"{_audioBaseUrl}{c}-default.wav");
                    }
                }
            }

            return audioFiles;
        }
    }
}
