using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interfaces;
using SystemQMS.ViewModels.NotificationDto;

namespace SystemQMS.Services
{
    public class NotificationSvc : INotificationSvc
    {
        private readonly MyDbContext _context;

        public NotificationSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllAsync(int unitId)
        {
            return await _context.Set<Notification>()
                .Where(x => x.UnitId == unitId)
                .OrderByDescending(x => x.TimeSend)
                .Select(x => new NotificationResponseDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Content = x.Content,
                    Form = x.Form,
                    Type = x.Type,
                    TimeSend = x.TimeSend,
                    Status = x.Status,
                    AttachmentUrl = x.AttachmentUrl
                })
                .ToListAsync();
        }

        public async Task<NotificationResponseDto> GetByIdAsync(int id, int unitId)
        {
            var noti = await _context.Set<Notification>()
                .Include(x => x.Histories)
                .ThenInclude(h => h.Guest)
                .FirstOrDefaultAsync(x => x.Id == id && x.UnitId == unitId);

            if (noti == null) return null;

            return new NotificationResponseDto
            {
                Id = noti.Id,
                Title = noti.Title,
                Content = noti.Content,
                Form = noti.Form,
                Type = noti.Type,
                TimeSend = noti.TimeSend,
                Status = noti.Status,
                AttachmentUrl = noti.AttachmentUrl,
                Recipients = noti.Histories.Select(h => new NotificationHistoryDto
                {
                    GuestId = h.GuestId,
                    GuestName = h.Guest != null ? h.Guest.GuestName : string.Empty,
                    GuestEmail = h.Guest != null ? h.Guest.Email : string.Empty,
                    Status = h.Status
                }).ToList()
            };
        }

        public async Task<NotificationResponseDto> CreateAsync(int unitId, NotificationCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Tiêu đề không được để trống.");

            var notification = new Notification
            {
                Title = dto.Title.Trim(),
                Content = dto.Content,
                Form = dto.Form,
                Type = dto.Type,
                TimeSend = dto.Type == "Ngay lập tức" ? DateTimeOffset.UtcNow : dto.TimeSend,
                Status = "Chờ gửi",
                UnitId = unitId,
                AttachmentUrl = dto.AttachmentUrl ?? ""
            };

            _context.Set<Notification>().Add(notification);

            await _context.SaveChangesAsync();

            var distinctGuestIds = dto.GuestIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (distinctGuestIds.Any())
            {
                var histories = distinctGuestIds.Select(guestId => new History
                {
                    NotiId = notification.Id,
                    GuestId = guestId,
                    Status = "Chờ gửi"
                }).ToList();

                _context.Set<History>().AddRange(histories);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(notification.Id, unitId);
        }

        public async Task<bool> UpdateAsync(int id, int unitId, NotificationUpdateDto dto)
        {
            var noti = await _context.Set<Notification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.UnitId == unitId);

            if (noti == null) return false;

            if (noti.Status != "Chờ gửi" && noti.Status != "Nháp")
                throw new InvalidOperationException("Chỉ được sửa thông báo khi chưa gửi hoặc đang là bản nháp.");

            noti.Title = dto.Title.Trim();
            noti.Content = dto.Content;
            noti.Type = dto.Type;
            noti.AttachmentUrl = dto.AttachmentUrl ?? "";

            if (dto.Type != "Ngay lập tức")
            {
                noti.TimeSend = dto.TimeSend;
            }

            _context.Set<Notification>().Update(noti);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int unitId)
        {
            var noti = await _context.Set<Notification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.UnitId == unitId);

            if (noti == null) return false;

            if (noti.Status != "Chờ gửi" && noti.Status != "Nháp")
                throw new InvalidOperationException("Không thể xóa thông báo đã được thực thi.");

            _context.Set<Notification>().Remove(noti);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RetryFailedAsync(int id, int unitId)
        {
            var histories = await _context.Set<History>()
                .Include(h => h.Notification)
                .Where(h => h.NotiId == id && h.Notification.UnitId == unitId && (h.Status == "Thất bại" || h.Status == "Gửi lỗi"))
                .ToListAsync();

            if (!histories.Any()) return false;

            foreach (var history in histories)
            {
                history.Status = "Chờ gửi";
            }

            var noti = await _context.Set<Notification>().FindAsync(id);
            if (noti != null && (noti.Status == "Gửi lỗi" || noti.Status == "Thất bại"))
            {
                noti.Status = "Đang xử lý";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}