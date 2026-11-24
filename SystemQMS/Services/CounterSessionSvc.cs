using Microsoft.EntityFrameworkCore;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Services
{
    public class CounterSessionSvc : ICounterSessionSvc
    {
        private readonly MyDbContext _context;

        public CounterSessionSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, CounterSessionResponseDto? Data)> OpenSessionAsync(int counterId, OpenSessionRequestDto dto)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null) return (false, $"Không tìm thấy quầy có ID = {counterId}.", null);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.userId && u.IsActive);
            if (user == null) return (false, "Tài khoản cán bộ không tồn tại hoặc đã bị khóa.", null);

            if (counter.UnitId != user.UnitId) return (false, "Cán bộ không thuộc đơn vị quản lý của quầy này.", null);


            var currentStaff = await _context.Users.FirstOrDefaultAsync(u => u.CounterId == counterId && u.UserId != dto.userId);
            if (currentStaff != null) return (false, $"Quầy hiện đang có cán bộ '{currentStaff.Username}' trực.", null);

            if (user.CounterId > 0 && user.CounterId != counterId)
                return (false, $"Cán bộ đang mở phiên tại Quầy ID = {user.CounterId}. Vui lòng đăng xuất quầy cũ trước.", null);

            user.CounterId = counterId;
            counter.Status = CounterStatus.Idle;

            await _context.SaveChangesAsync();

            return (true, $"Mở phiên trực thành công cho cán bộ {user.Username}.", new CounterSessionResponseDto
            {
                counterId = counter.Id,
                counterCode = counter.CounterId,
                counterName = counter.CounterName ?? "Quầy giao dịch",
                status = counter.Status,
                isActive = counter.IsActive,
                currentUserId = user.UserId,
                currentUserName = user.Username,
                sessionStartTime = DateTimeOffset.UtcNow
            });
        }

        public async Task<(bool Success, string Message)> CloseSessionAsync(int counterId, int userId)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.CounterId == counterId);
            if (user == null) return (false, "Cán bộ không trực tại quầy này.");

            user.CounterId = 0;
            counter.Status = CounterStatus.Closed;
            await _context.SaveChangesAsync();
            return (true, "Đã đóng phiên trực và đăng xuất khỏi quầy.");
        }

        public async Task<CounterSessionResponseDto?> GetCurrentSessionAsync(int counterId)
        {
            var counter = await _context.Counters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null) return null;

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.CounterId == counterId);
            return new CounterSessionResponseDto
            {
                counterId = counter.Id,
                counterCode = counter.CounterId,
                counterName = counter.CounterName ?? "Trống",
                status = counter.Status,
                isActive = counter.IsActive,
                currentUserId = user?.UserId,
                currentUserName = user?.Username,
                sessionStartTime = DateTimeOffset.UtcNow
            };
        }

        public async Task<(bool Success, string Message)> CloseCounterAsync(int counterId)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            // 1. Chặn đóng quầy nếu có yêu cầu chuyển vé đang ở trạng thái Pending[cite: 3]
            bool hasPendingTransfer = await _context.CounterChanges
                .AnyAsync(cc => cc.CounterIdSender == counterId && cc.Status == CounterChangeStatus.Pending);

            if (hasPendingTransfer)
            {
                return (false, "Không thể đóng quầy! Quầy đang có yêu cầu chuyển phiếu chờ quầy khác tiếp nhận (Pending).");
            }

            // 2. Chỉ chuyển status runtime sang Closed, giữ nguyên cờ isActive
            counter.Status = CounterStatus.Closed;
            await _context.SaveChangesAsync();
            return (true, "Đã đóng quầy thành công.");
        }

        public async Task<(bool Success, string Message)> OpenCounterAsync(int counterId)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            if (!counter.IsActive)
                return (false, "Quầy đã bị vô hiệu hóa cấu hình quản trị (isActive = false). Không thể mở quầy!");

            counter.Status = CounterStatus.Idle;
            await _context.SaveChangesAsync();
            return (true, "Mở quầy thành công, sẵn sàng nhận khách.");
        }
    }
}