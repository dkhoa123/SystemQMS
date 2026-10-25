
using Microsoft.AspNetCore.SignalR;
using SystemQMS.Dtos;
using SystemQMS.Models;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;

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
            var unit = await _context.units
                .Include(u => u.UnitConfig)
                .FirstOrDefaultAsync(u => u.unitId == unitId);

            if (unit == null) return null;

            return new DisplayConfigResponseDto
            {
                UnitName = unit.unitName,
                LogoUrl = unit.logo != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(unit.logo)}" : null,
                BackgroundImageUrl = unit.UnitConfig?.BackgroundImage,
                BannerText = unit.UnitConfig?.BannerText,
                ToneColorHex = unit.UnitConfig?.ToneColorHex,
                EvaluationTemplateId = unit.UnitConfig?.ActiveEvaluationTemplateId
            };
        }

        public async Task<DisplayConfigResponseDto> UpdateDisplayConfigAsync(int unitId, DisplayConfigUpdateDto dto)
        {
            var unit = await _context.units
                .Include(u => u.UnitConfig)
                .FirstOrDefaultAsync(u => u.unitId == unitId);

            if (unit == null) throw new Exception("Unit not found");

            // 1. Cập nhật bảng Unit
            if (!string.IsNullOrEmpty(dto.UnitName))
                unit.unitName = dto.UnitName;

            if (dto.LogoFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Đọc dữ liệu từ file upload vào bộ nhớ
                    await dto.LogoFile.CopyToAsync(memoryStream);

                    // Gán mảng byte vào thuộc tính logo của database
                    unit.logo = memoryStream.ToArray();
                }
            }

            // 2. Khởi tạo hoặc Cập nhật bảng UnitConfig
            if (unit.UnitConfig == null)
            {
                unit.UnitConfig = new UnitConfig { UnitId = unitId };
                _context.unitConfigs.Add(unit.UnitConfig);
            }

            if (dto.BackgroundFile != null)
            {
                var bgPath = await SaveFileAsync(dto.BackgroundFile, "backgrounds");
                unit.UnitConfig.BackgroundImage = bgPath;
            }

            if (dto.BannerText != null) unit.UnitConfig.BannerText = dto.BannerText;
            if (dto.ToneColorHex != null) unit.UnitConfig.ToneColorHex = dto.ToneColorHex;
            if (dto.EvaluationTemplateId.HasValue) unit.UnitConfig.ActiveEvaluationTemplateId = dto.EvaluationTemplateId;

            await _context.SaveChangesAsync();

            // 3. Trigger Real-time cho LCD và Kiosk[cite: 34]
            await _hubContext.Clients.Group($"Unit_{unitId}").SendAsync("OnDisplayConfigChanged");

            return await GetDisplayConfigAsync(unitId);
        }

        public async Task ResetDisplayFieldAsync(int unitId, string fieldName)
        {
            var unit = await _context.units.Include(u => u.UnitConfig).FirstOrDefaultAsync(u => u.unitId == unitId);
            if (unit == null) return;

            // Xóa thông tin đã nhập để quay về mặc định[cite: 8]
            switch (fieldName.ToLower())
            {
                case "background":
                    if (unit.UnitConfig != null) unit.UnitConfig.BackgroundImage = null;
                    break;
                case "logo":
                    unit.logo = null;
                    break;
                case "bannertext":
                    if (unit.UnitConfig != null) unit.UnitConfig.BannerText = null;
                    break;
                case "tonecolor":
                    if (unit.UnitConfig != null) unit.UnitConfig.ToneColorHex = null;
                    break;
            }
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group($"Unit_{unitId}").SendAsync("OnDisplayConfigChanged");
        }

        public async Task UpdateAudioConfigAsync(int unitId, int templateId)
        {
            var config = await _context.unitConfigs.FirstOrDefaultAsync(c => c.UnitId == unitId);
            if (config == null)
            {
                config = new UnitConfig { UnitId = unitId, ActiveAudioTemplateId = templateId };
                _context.unitConfigs.Add(config);
            }
            else
            {
                config.ActiveAudioTemplateId = templateId;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            // Logic lưu file tĩnh vào thư mục wwwroot/{folder} và trả về đường dẫn URL
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/{folder}/{fileName}";
        }
    }        
}
