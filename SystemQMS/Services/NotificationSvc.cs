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
                .Where(x => x.unitId == unitId)
                .OrderByDescending(x => x.timeSend)
                .Select(x => new NotificationResponseDto
                {
                    Id = x.Id,
                    Title = x.title,
                    Content = x.content,
                    Form = x.form,
                    Type = x.type,
                    TimeSend = x.timeSend,
                    Status = x.status,
                    AttachmentUrl = x.AttachmentUrl
                })
                .ToListAsync();
        }

        public async Task<NotificationResponseDto> GetByIdAsync(int id, int unitId)
        {
            var noti = await _context.Set<Notification>()
                .Include(x => x.Histories)
                .ThenInclude(h => h.Guest)
                .FirstOrDefaultAsync(x => x.Id == id && x.unitId == unitId);

            if (noti == null) return null;

            return new NotificationResponseDto
            {
                Id = noti.Id,
                Title = noti.title,
                Content = noti.content,
                Form = noti.form,
                Type = noti.type,
                TimeSend = noti.timeSend,
                Status = noti.status,
                AttachmentUrl = noti.AttachmentUrl,
                Recipients = noti.Histories.Select(h => new NotificationHistoryDto
                {
                    GuestId = h.guestId,
                    GuestName = h.Guest != null ? h.Guest.guestName : string.Empty,
                    GuestEmail = h.Guest != null ? h.Guest.email : string.Empty,
                    Status = h.status
                }).ToList()
            };
        }

        public async Task<NotificationResponseDto> CreateAsync(int unitId, NotificationCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Tiêu đề không được để trống.");

            var notification = new Notification
            {
                title = dto.Title.Trim(),
                content = dto.Content,
                form = dto.Form,
                type = dto.Type,
                timeSend = dto.Type == "Ngay lập tức" ? DateTimeOffset.UtcNow : dto.TimeSend,
                status = "Chờ gửi",
                unitId = unitId,
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
                    notiId = notification.Id,
                    guestId = guestId,
                    status = "Chờ gửi"
                }).ToList();

                _context.Set<History>().AddRange(histories);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(notification.Id, unitId);
        }

        public async Task<bool> UpdateAsync(int id, int unitId, NotificationUpdateDto dto)
        {
            var noti = await _context.Set<Notification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.unitId == unitId);

            if (noti == null) return false;

            if (noti.status != "Chờ gửi" && noti.status != "Nháp")
                throw new InvalidOperationException("Chỉ được sửa thông báo khi chưa gửi hoặc đang là bản nháp.");

            noti.title = dto.Title.Trim();
            noti.content = dto.Content;
            noti.type = dto.Type;
            noti.AttachmentUrl = dto.AttachmentUrl ?? "";

            if (dto.Type != "Ngay lập tức")
            {
                noti.timeSend = dto.TimeSend;
            }

            _context.Set<Notification>().Update(noti);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int unitId)
        {
            var noti = await _context.Set<Notification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.unitId == unitId);

            if (noti == null) return false;

            if (noti.status != "Chờ gửi" && noti.status != "Nháp")
                throw new InvalidOperationException("Không thể xóa thông báo đã được thực thi.");

            _context.Set<Notification>().Remove(noti);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RetryFailedAsync(int id, int unitId)
        {
            var histories = await _context.Set<History>()
                .Include(h => h.Notification)
                .Where(h => h.notiId == id && h.Notification.unitId == unitId && (h.status == "Thất bại" || h.status == "Gửi lỗi"))
                .ToListAsync();

            if (!histories.Any()) return false;

            foreach (var history in histories)
            {
                history.status = "Chờ gửi";
            }

            var noti = await _context.Set<Notification>().FindAsync(id);
            if (noti != null && (noti.status == "Gửi lỗi" || noti.status == "Thất bại"))
            {
                noti.status = "Đang xử lý";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}