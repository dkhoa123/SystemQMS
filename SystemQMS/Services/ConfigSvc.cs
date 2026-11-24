
using Microsoft.AspNetCore.SignalR;
using SystemQMS.Models;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;
using SystemQMS.ViewModels.AudioDto;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class ConfigService : IConfigSvc
    {
        private readonly MyDbContext _context;
        private readonly IHubContext<QmsHub> _hubContext;

        public ConfigService(MyDbContext context, IHubContext<QmsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<DisplayConfigResponseDto> GetDisplayConfigAsync(int unitId)
        {
            var unit = await _context.Units
                .Include(u => u.UnitConfig)
                .FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (unit == null) return null;

            return new DisplayConfigResponseDto
            {
                UnitName = unit.UnitName,
                LogoUrl = unit.Logo != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(unit.Logo)}" : null,
                BackgroundImageUrl = unit.UnitConfig?.BackgroundImage,
                BannerText = unit.UnitConfig?.BannerText,
                ToneColorHex = unit.UnitConfig?.ToneColorHex,
                EvaluationTemplateId = unit.UnitConfig?.ActiveEvaluationTemplateId
            };
        }

        public async Task<DisplayConfigResponseDto> UpdateDisplayConfigAsync(int unitId, DisplayConfigUpdateDto dto)
        {
            var unit = await _context.Units
                .Include(u => u.UnitConfig)
                .FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (unit == null) throw new Exception("Không tìm thấy đơn vị.");

            if (unit.UnitConfig == null)
            {
                unit.UnitConfig = new UnitConfig
                {
                    UnitId = unitId,
                    BackgroundImage = "",
                    BannerText = "",
                    ToneColorHex = ""
                };
                _context.UnitConfigs.Add(unit.UnitConfig);
            }

            if (!string.IsNullOrEmpty(dto.UnitName))
            {
                unit.UnitName = dto.UnitName;
            }
            if (dto.BannerText != null) unit.UnitConfig.BannerText = dto.BannerText;
            if (dto.ToneColorHex != null) unit.UnitConfig.ToneColorHex = dto.ToneColorHex;

            if (dto.BackgroundFile != null)
            {
                unit.UnitConfig.BackgroundImage = await SaveFileAsync(dto.BackgroundFile, "backgrounds");
            }

            if (dto.LogoFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await dto.LogoFile.CopyToAsync(memoryStream);
                    unit.Logo = memoryStream.ToArray();
                }
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Unit_{unitId}").SendAsync("OnDisplayConfigChanged");

            return await GetDisplayConfigAsync(unitId);
        }

        public async Task ResetDisplayFieldAsync(int unitId, string fieldName)
        {
            var unit = await _context.Units.Include(u => u.UnitConfig).FirstOrDefaultAsync(u => u.UnitId == unitId);
            if (unit == null) return;

            switch (fieldName.ToLower())
            {
                case "background":
                    if (unit.UnitConfig != null) unit.UnitConfig.BackgroundImage = "";
                    break;
                case "logo":
                    unit.Logo = null;
                    break;
                case "bannertext":
                    if (unit.UnitConfig != null) unit.UnitConfig.BannerText = "";
                    break;
                case "tonecolor":
                    if (unit.UnitConfig != null) unit.UnitConfig.ToneColorHex = "";
                    break;
            }
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"Unit_{unitId}").SendAsync("OnDisplayConfigChanged");
        }

        public async Task UpdateAudioConfigAsync(int unitId, int templateId)
        {
            var config = await _context.UnitConfigs.FirstOrDefaultAsync(c => c.UnitId == unitId);
            if (config == null)
            {
                config = new UnitConfig
                {
                    UnitId = unitId,
                    ActiveAudioTemplateId = templateId,
                    BackgroundImage = "",
                    BannerText = "",
                    ToneColorHex = ""
                };
                _context.UnitConfigs.Add(config);
            }
            else
            {
                config.ActiveAudioTemplateId = templateId;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/{folder}/{fileName}";
        }
        private static readonly double[] AllowedSpeeds = { 0.5, 1.0, 1.5, 2.0 };

        public async Task<AudioConfigResponseDto> GetAudioConfigAsync(int unitId)
        {
            var config = await _context.UnitConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UnitId == unitId);

            return new AudioConfigResponseDto
            {
                TemplateId = config?.ActiveAudioTemplateId ?? 1,
                ReadingSpeed = config?.ReadingSpeed ?? 1.0
            };
        }

        public async Task UpdateAudioConfigAsync(int unitId, AudioConfigUpdateDto dto)
        {
            if (dto.TemplateId < 1 || dto.TemplateId > 3)
                throw new ArgumentException("Mẫu câu đọc chỉ chấp nhận giá trị 1, 2 hoặc 3.");

            if (!AllowedSpeeds.Contains(dto.ReadingSpeed))
                throw new ArgumentException("Tốc độ đọc chỉ chấp nhận: 0.5, 1, 1.5, 2.");

            var config = await _context.UnitConfigs.FirstOrDefaultAsync(c => c.UnitId == unitId);
            if (config == null)
            {
                config = new UnitConfig
                {
                    UnitId = unitId,
                    ActiveAudioTemplateId = dto.TemplateId,
                    ReadingSpeed = dto.ReadingSpeed,
                    BackgroundImage = "",
                    BannerText = "",
                    ToneColorHex = ""
                };
                _context.UnitConfigs.Add(config);
            }
            else
            {
                config.ActiveAudioTemplateId = dto.TemplateId;
                config.ReadingSpeed = dto.ReadingSpeed;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Unit_{unitId}").SendAsync("OnAudioConfigChanged", new
            {
                TemplateId = config.ActiveAudioTemplateId,
                ReadingSpeed = config.ReadingSpeed
            });
        }
    }        
}
