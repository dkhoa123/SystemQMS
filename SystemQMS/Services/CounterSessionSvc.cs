using Microsoft.EntityFrameworkCore;
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
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return (false, $"Không tìm thấy quầy có ID = {counterId}.", null);

            var user = await _context.users.FirstOrDefaultAsync(u => u.userId == dto.userId && u.isActive);
            if (user == null) return (false, "Tài khoản cán bộ không tồn tại hoặc đã bị khóa.", null);

            if (counter.unitId != user.unitId) return (false, "Cán bộ không thuộc đơn vị quản lý của quầy này.", null);

            var currentStaff = await _context.users.FirstOrDefaultAsync(u => u.counterId == counterId && u.userId != dto.userId);
            if (currentStaff != null) return (false, $"Quầy hiện đang có cán bộ '{currentStaff.username}' trực.", null);

            if (user.counterId > 0 && user.counterId != counterId)
                return (false, $"Cán bộ đang mở phiên tại Quầy ID = {user.counterId}. Vui lòng đăng xuất quầy cũ trước.", null);

            user.counterId = counterId;
            counter.status = CounterStatus.Idle;

            await _context.SaveChangesAsync();

            return (true, $"Mở phiên trực thành công cho cán bộ {user.username}.", new CounterSessionResponseDto
            {
                counterId = counter.id,
                counterCode = counter.counterId,
                counterName = counter.counterName ?? "Quầy giao dịch",
                status = counter.status,
                isActive = counter.isActive,
                currentUserId = user.userId,
                currentUserName = user.username,
                sessionStartTime = DateTimeOffset.UtcNow
            });
        }

        public async Task<(bool Success, string Message)> CloseSessionAsync(int counterId, int userId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            var user = await _context.users.FirstOrDefaultAsync(u => u.userId == userId && u.counterId == counterId);
            if (user == null) return (false, "Cán bộ không trực tại quầy này.");

            user.counterId = 0;
            counter.status = CounterStatus.Closed;
            await _context.SaveChangesAsync();
            return (true, "Đã đóng phiên trực và đăng xuất khỏi quầy.");
        }

        public async Task<CounterSessionResponseDto?> GetCurrentSessionAsync(int counterId)
        {
            var counter = await _context.counters.AsNoTracking().FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return null;

            var user = await _context.users.AsNoTracking().FirstOrDefaultAsync(u => u.counterId == counterId);
            return new CounterSessionResponseDto
            {
                counterId = counter.id,
                counterCode = counter.counterId,
                counterName = counter.counterName ?? "Trống",
                status = counter.status,
                isActive = counter.isActive,
                currentUserId = user?.userId,
                currentUserName = user?.username,
                sessionStartTime = DateTimeOffset.UtcNow
            };
        }

        public async Task<(bool Success, string Message)> CloseCounterAsync(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            // 1. Chặn đóng quầy nếu có yêu cầu chuyển vé đang ở trạng thái Pending[cite: 3]
            bool hasPendingTransfer = await _context.counterChanges
                .AnyAsync(cc => cc.counterIdSender == counterId && cc.status == CounterChangeStatus.Pending);

            if (hasPendingTransfer)
            {
                return (false, "Không thể đóng quầy! Quầy đang có yêu cầu chuyển phiếu chờ quầy khác tiếp nhận (Pending).");
            }

            // 2. Chỉ chuyển status runtime sang Closed, giữ nguyên cờ isActive
            counter.status = CounterStatus.Closed;
            await _context.SaveChangesAsync();
            return (true, "Đã đóng quầy thành công.");
        }

        public async Task<(bool Success, string Message)> OpenCounterAsync(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return (false, "Không tìm thấy quầy.");

            if (!counter.isActive)
                return (false, "Quầy đã bị vô hiệu hóa cấu hình quản trị (isActive = false). Không thể mở quầy!");

            counter.status = CounterStatus.Idle;
            await _context.SaveChangesAsync();
            return (true, "Mở quầy thành công, sẵn sàng nhận khách.");
        }
    }
}