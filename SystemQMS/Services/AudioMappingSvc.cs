using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.AudioDto;

namespace SystemQMS.Services
{
    public class AudioMappingSvc : IAudioMappingSvc
    {
        private readonly string _audioBaseUrl = "/AudioConfigs/";
        private readonly MyDbContext _context;

        public AudioMappingSvc(MyDbContext context)
        {
            _context = context;
        }

        public List<string> BuildAudioSequence(string ticketNumber, string counterIdentifier, int templateId)
        {
            var audioFiles = new List<string>();

            if (templateId == 1 || templateId == 3)
            {
                audioFiles.Add($"{_audioBaseUrl}welcome.wav");
            }

            AppendAlphanumericAudio(audioFiles, ticketNumber);

            if (templateId == 3) return audioFiles;

            if (templateId == 1 || templateId == 2)
            {
                audioFiles.Add($"{_audioBaseUrl}come-to.wav");
            }

            AppendAlphanumericAudio(audioFiles, counterIdentifier);

            return audioFiles;
        }

        public async Task<AudioCallEventDto> BuildAudioCallEventAsync(
            int unitId,
            int counterId,
            string counterName,
            string ticketNumber,
            int repeatCount = 1)
        {
            var config = await _context.UnitConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UnitId == unitId);

            int templateId = config?.ActiveAudioTemplateId ?? 1;
            double speed = config?.ReadingSpeed ?? 1.0;

            var sequence = BuildAudioSequence(ticketNumber, counterName, templateId);

            return new AudioCallEventDto
            {
                CounterId = counterId,
                CounterName = counterName,
                TicketNumber = ticketNumber,
                AudioFiles = sequence,
                ReadingSpeed = speed,
                RepeatCount = repeatCount
            };
        }

        private void AppendAlphanumericAudio(List<string> audioFiles, string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            foreach (char c in input.ToUpperInvariant())
            {
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    audioFiles.Add($"{_audioBaseUrl}{c}-default.wav");
                }
            }
        }
    }
}