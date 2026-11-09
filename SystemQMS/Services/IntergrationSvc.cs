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
            var query = _context.guests.Where(g => g.unitId == unitId);

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(g => g.guestName.ToLower().Contains(lowerKeyword) ||
                                         g.email.ToLower().Contains(lowerKeyword) ||
                                         g.phoneNumber.Contains(keyword));
            }

            return await query.Select(g => new CustomerDto
            {
                Id = g.id,
                GuestId = g.guestId,
                GuestName = g.guestName,
                Email = g.email,
                PhoneNumber = g.phoneNumber
            }).ToListAsync();
        }

        public async Task<List<ServiceScheduleDto>> GetQmsServicesAsync(int unitId)
        {
            var services = await _context.services
                .Include(s => s.SheduleActives)
                .Where(s => s.unitId == unitId && s.isActive)
                .ToListAsync();

            return services.Select(s => new ServiceScheduleDto
            {
                ServiceId = s.id,
                ServiceCode = s.serviceId,
                ServiceName = s.serviceName,
                Schedules = s.SheduleActives.Select(sch => new ScheduleDto
                {
                    StartTime = sch.startTime,
                    EndTime = sch.endTime,
                    WordShift = sch.wordShift
                }).ToList()
            }).ToList();
        }
    }
}