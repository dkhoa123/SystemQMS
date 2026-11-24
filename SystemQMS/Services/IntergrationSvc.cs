using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interfaces;
using SystemQMS.ViewModels.IntergrationsDto;

namespace SystemQMS.Services
{
    public class IntegrationSvc : IIntegrationSvc
    {
        private readonly MyDbContext _context;

        public IntegrationSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerDto>> GetCrmCustomersAsync(int unitId, string keyword)
        {
            var query = _context.Guests.Where(g => g.UnitId == unitId);

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(g => g.GuestName.ToLower().Contains(lowerKeyword) ||
                                         g.Email.ToLower().Contains(lowerKeyword) ||
                                         g.PhoneNumber.Contains(keyword));
            }

            return await query.Select(g => new CustomerDto
            {
                Id = g.Id,
                GuestId = g.GuestId,
                GuestName = g.GuestName,
                Email = g.Email,
                PhoneNumber = g.PhoneNumber
            }).ToListAsync();
        }

        public async Task<List<ServiceScheduleDto>> GetQmsServicesAsync(int unitId)
        {
            var services = await _context.Services
                .Include(s => s.SheduleActives)
                .Where(s => s.UnitId == unitId && s.IsActive)
                .ToListAsync();

            return services.Select(s => new ServiceScheduleDto
            {
                ServiceId = s.Id,
                ServiceCode = s.ServiceId,
                ServiceName = s.ServiceName,
                Schedules = s.SheduleActives.Select(sch => new ScheduleDto
                {
                    StartTime = sch.StartTime,
                    EndTime = sch.EndTime,
                    WordShift = sch.WordShift
                }).ToList()
            }).ToList();
        }
    }
}