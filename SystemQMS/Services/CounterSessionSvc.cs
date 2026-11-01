using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CounterSessionDto;

namespace SystemQMS.Services
{
    public class CounterSessionSvc : ICounterSessionSvc
    {
        private MyDbContext _context;
        public CounterSessionSvc (MyDbContext context)
        {
            _context = context;
        }

        public async Task<string> ReleaseControl(int counterId, int userId)
        {
            var session = await _context.counterSessions
                .FirstOrDefaultAsync(x =>
                         x.counterId == counterId &&
                         x.userId == userId &&
                         x.isActive == true);
            if (session == null)
                return "Không tìm thấy phiên điều khiển đang hoạt động";
            session.isActive = false;
            return "Trả quyền điều khiển thành công ";
        }

        public async Task<string> TakeControl(CounterSessionDto model)
        {
            var couter = await _context.counters.FirstOrDefaultAsync(x => x.id == model.counterId);
            if (couter == null)
                return "Quầy không tồn tại .";
            var user = await _context.users.FirstOrDefaultAsync(x => x.userId == model.userId);
            if (user == null)
                return "Nhân viên không tồn tại .";
            var device = await _context.devices.FirstOrDefaultAsync(x => x.id == model.deviceId);
            if (device == null)
                return "Thiết bị không tồn tại .";

            var currentSession = await _context.counterSessions
                .FirstOrDefaultAsync(x => x.counterId == model.counterId && x.isActive == true);
            if (currentSession != null && currentSession.userId == model.userId)
                return " Nhân viên đang điều khiển quầy này .";

            // ket thuc phien

            if(currentSession!= null )
            {
                currentSession.isActive = false;
                currentSession.endedAt = DateTime.Now;
            }

            // tao phien
            var newSession = new CounterSession
            {
                counterId = model.counterId,
                userId = model.userId,
                deviceId = model.deviceId,
                startedAt = DateTime.Now,
                isActive = true
            };

            await _context.counterSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();


            return "Nhận quyền điều khiển quầy thành công";
        }
    }
}
