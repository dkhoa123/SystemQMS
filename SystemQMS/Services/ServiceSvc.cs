using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Services
{
    public class ServiceSvc : IServiceSvc
    {
        private readonly MyDbContext _context;

        public ServiceSvc(MyDbContext context)
        {
            _context = context;
        }

        // SVC-01: Lấy danh sách dịch vụ kèm bộ lọc
        public async Task<IEnumerable<ServiceResponseDto>> GetAllServicesAsync(int? unitId, int? counterId, bool? isActive, string? search)
        {
            var query = _context.Services.AsNoTracking().AsQueryable();

            if (unitId.HasValue)
                query = query.Where(s => s.UnitId == unitId.Value);

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            // Bổ sung lọc theo counterId (kiểm tra qua danh sách vé hoặc danh sách quầy liên kết)
            if (counterId.HasValue)
            {
                query = query.Where(s => s.Counters.Any(c => c.Id == counterId.Value) || 
                                         s.Tickets.Any(st => st.CounterId == counterId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(s => s.ServiceName.ToLower().Contains(keyword) || 
                                         s.ServiceId.ToLower().Contains(keyword));
            }

            return await query.Select(s => new ServiceResponseDto
            {
                id = s.Id,
                unitId = s.UnitId,
                serviceId = s.ServiceId,
                serviceName = s.ServiceName,
                description = s.Description,
                image = s.Image,
                isActive = s.IsActive,
                sharingLink = s.SharingLink,
                averageTime = s.AverageTime,
                totalTicketsIssued = _context.Tickets.Count(t => t.ServiceId == s.Id)
            }).ToListAsync();
        }

        // SVC-05: Xem chi tiết dịch vụ đầy đủ (bao gồm quầy tiếp nhận và danh sách STT)
        public async Task<ServiceDetailDto?> GetServiceByIdAsync(int id, string? ticketStatus = null, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var service = await _context.Services
                .AsNoTracking()
                .Include(s => s.Counters)
                .Include(s => s.Tickets)
                    .ThenInclude(st => st.Counter)
                .Include(s => s.Tickets)
                    .ThenInclude(st => st.Users)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return null;

            // Lọc danh sách STT
            var ticketsQuery = service.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketStatus))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status != null && t.Status.ToLower() == ticketStatus.Trim().ToLower());
            }

            if (fromDate.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.IssuanceTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.IssuanceTime <= toDate.Value);
            }

            var ticketItems = ticketsQuery.Select(t => new ServiceTicketItemDto
            {
                id = t.SttId,
                ticketNumber = t.SttId,
                status = t.Status,
                stage = t.Stage,
                handlerUser = t.Users != null ? t.Users.Username : "Chưa xử lý",
                issuanceTime = t.IssuanceTime,
                useTime = t.UseTime,
                counterName = t.Counter != null ? t.Counter.CounterName : "Trống"
            }).ToList();

            // Tổng hợp danh sách quầy tiếp nhận dịch vụ
            var assignedCounters = service.Counters.Select(c => new ServiceCounterItemDto
            {
                id = c.Id,
                counterId = c.CounterId,
                counterName = c.CounterName,
                isActive = c.IsActive,
                status = c.Status
            }).ToList();

            // Nếu quan hệ Nhiều-Nhiều chưa có dữ liệu, lấy bù từ lịch sử quầy đã từng cấp vé
            if (!assignedCounters.Any())
            {
                assignedCounters = service.Tickets
                    .Where(t => t.Counter != null)
                    .Select(t => t.Counter!)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .Select(c => new ServiceCounterItemDto
                    {
                        id = c.Id,
                        counterId = c.CounterId,
                        counterName = c.CounterName,
                        isActive = c.IsActive,
                        status = c.Status
                    }).ToList();
            }

            return new ServiceDetailDto
            {
                id = service.Id,
                unitId = service.UnitId,
                serviceId = service.ServiceId,
                serviceName = service.ServiceName,
                description = service.Description,
                image = service.Image,
                isActive = service.IsActive,
                sharingLink = service.SharingLink,
                averageTime = service.AverageTime,
                totalTicketsIssued = service.Tickets.Count,
                assignedCounters = assignedCounters,
                issuedTickets = ticketItems
            };
        }

        // SVC-02 & SVC-03: Thêm mới dịch vụ
        public async Task<(bool Success, string Message, int? NewId, string? ServiceId)> CreateServiceAsync(CreateServiceDto dto)
        {
            bool unitExists = await _context.Units.AnyAsync(u => u.UnitId == dto.unitId);
            if (!unitExists)
                return (false, $"Đơn vị có ID = {dto.unitId} không tồn tại trong hệ thống.", null, null);

            bool isDuplicate = await _context.Services.AnyAsync(s =>
                s.UnitId == dto.unitId && s.ServiceId.ToLower() == dto.serviceId.Trim().ToLower());

            if (isDuplicate)
                return (false, $"Mã dịch vụ '{dto.serviceId}' đã tồn tại trong đơn vị.", null, null);

            var service = new Service
            {
                UnitId = dto.unitId,
                ServiceId = dto.serviceId.Trim(),
                ServiceName = dto.serviceName.Trim(),
                Description = dto.description,
                Image = dto.image,
                IsActive = dto.isActive,
                SharingLink = dto.sharingLink,
                AverageTime = dto.averageTime
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return (true, "Thêm dịch vụ thành công.", service.Id, service.ServiceId);
        }

        // SVC-04: Cập nhật thông tin dịch vụ
        public async Task<(bool Success, string Message)> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {id}");

            service.ServiceName = dto.serviceName.Trim();
            service.Description = dto.description;
            service.IsActive = dto.isActive;
            service.SharingLink = dto.sharingLink;
            service.AverageTime = dto.averageTime;
            if (dto.image != null)
            {
                service.Image = dto.image;
            }

            await _context.SaveChangesAsync();
            return (true, "Cập nhật dịch vụ thành công.");
        }

        // SVC-06: Xóa dịch vụ (Bảo vệ toàn vẹn lịch sử vé)
        public async Task<(bool Success, string Message)> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {id}");

            bool hasTickets = await _context.Tickets.AnyAsync(t => t.ServiceId == service.Id);
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.ServiceId == service.Id);

            if (hasTickets || hasBookings)
                return (false, "Không thể xóa dịch vụ này vì đã có phiếu STT hoặc lịch hẹn phát sinh. Hãy chuyển trạng thái sang 'Ngưng hoạt động'.");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return (true, "Xóa dịch vụ thành công.");
        }
    }
}