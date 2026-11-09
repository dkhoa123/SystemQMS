using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.ViewModels.WebConfigDto;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class WebConfigSvc : IWebConfigSvc
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public WebConfigSvc(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<WebConfigDto> GetWebConfigAsync(int unitId)
        {
            var unit = await _context.units
                .Include(u => u.UnitConfig)
                .FirstOrDefaultAsync(u => u.unitId == unitId);

            if (unit == null) return null;

            var dto = new WebConfigDto
            {
                UnitName = unit.unitName,
                ShowName = unit.showName,
                Address = unit.address,
                PhoneHelp = unit.phoneHelp,
                EmailHelp = unit.emailHelp,
            };

            if (unit.UnitConfig != null)
            {
                var config = unit.UnitConfig;
                dto.Favicon = config.Favicon;
                dto.Slogan = config.Slogan;
                dto.Website = config.Website;
                dto.TitleHome = config.TitleHome;
                dto.DescriptionHome = config.DescriptionHome;
                dto.BackgroundImage = config.BackgroundImage;
                dto.BannerText = config.BannerText;
                dto.ToneColorHex = config.ToneColorHex;

                dto.BookingMethod = config.BookingMethod;
                dto.SlotDuration = config.SlotDuration;
                dto.MaxGuestsPerSlot = config.MaxGuestsPerSlot;
                dto.AdvanceBookingDays = config.AdvanceBookingDays;
                dto.AllowImmediateBooking = config.AllowImmediateBooking;
                dto.MaxBookingsPerUserDaily = config.MaxBookingsPerUserDaily;
                dto.AppliedScope = config.AppliedScope;
            }

            return dto;
        }

        public async Task<bool> UpdateWebConfigAsync(int unitId, WebConfigDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var unit = await _context.units
                    .Include(u => u.UnitConfig)
                    .FirstOrDefaultAsync(u => u.unitId == unitId);

                if (unit == null) return false;

                unit.unitName = dto.UnitName;
                unit.showName = dto.ShowName;
                unit.address = dto.Address;
                unit.phoneHelp = dto.PhoneHelp;
                unit.emailHelp = dto.EmailHelp;

                if (unit.UnitConfig == null)
                {
                    unit.UnitConfig = new UnitConfig { UnitId = unitId };
                    _context.unitConfigs.Add(unit.UnitConfig);
                }

                unit.UnitConfig.Favicon = dto.Favicon;
                unit.UnitConfig.Slogan = dto.Slogan;
                unit.UnitConfig.Website = dto.Website;
                unit.UnitConfig.TitleHome = dto.TitleHome;
                unit.UnitConfig.DescriptionHome = dto.DescriptionHome;
                unit.UnitConfig.BackgroundImage = dto.BackgroundImage;
                unit.UnitConfig.BannerText = dto.BannerText;
                unit.UnitConfig.ToneColorHex = dto.ToneColorHex;

                unit.UnitConfig.BookingMethod = dto.BookingMethod;
                unit.UnitConfig.SlotDuration = dto.SlotDuration;
                unit.UnitConfig.MaxGuestsPerSlot = dto.MaxGuestsPerSlot;
                unit.UnitConfig.AdvanceBookingDays = dto.AdvanceBookingDays;
                unit.UnitConfig.AllowImmediateBooking = dto.AllowImmediateBooking;
                unit.UnitConfig.MaxBookingsPerUserDaily = dto.MaxBookingsPerUserDaily;
                unit.UnitConfig.AppliedScope = dto.AppliedScope;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không tồn tại hoặc rỗng");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".webp")
                throw new ArgumentException("Chỉ chấp nhận định dạng ảnh PNG, JPG, JPEG, WEBP");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{fileName}";
        }
    }
}