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
            var query = _context.services.AsNoTracking().AsQueryable();

            if (unitId.HasValue)
                query = query.Where(s => s.unitId == unitId.Value);

            if (isActive.HasValue)
                query = query.Where(s => s.isActive == isActive.Value);

            // Bổ sung lọc theo counterId (kiểm tra qua danh sách vé hoặc danh sách quầy liên kết)
            if (counterId.HasValue)
            {
                query = query.Where(s => s.counters.Any(c => c.id == counterId.Value) || 
                                         s.Stickers.Any(st => st.counterId == counterId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(s => s.serviceName.ToLower().Contains(keyword) || 
                                         s.serviceId.ToLower().Contains(keyword));
            }

            return await query.Select(s => new ServiceResponseDto
            {
                id = s.id,
                unitId = s.unitId,
                serviceId = s.serviceId,
                serviceName = s.serviceName,
                description = s.description,
                image = s.image,
                isActive = s.isActive,
                sharingLink = s.sharingLink,
                averageTime = s.averageTime,
                totalTicketsIssued = _context.stickers.Count(t => t.serviceId == s.id)
            }).ToListAsync();
        }

        // SVC-05: Xem chi tiết dịch vụ đầy đủ (bao gồm quầy tiếp nhận và danh sách STT)
        public async Task<ServiceDetailDto?> GetServiceByIdAsync(int id, string? ticketStatus = null, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var service = await _context.services
                .AsNoTracking()
                .Include(s => s.counters)
                .Include(s => s.Stickers)
                    .ThenInclude(st => st.Counter)
                .Include(s => s.Stickers)
                    .ThenInclude(st => st.Users)
                .FirstOrDefaultAsync(s => s.id == id);

            if (service == null) return null;

            // Lọc danh sách STT
            var ticketsQuery = service.Stickers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketStatus))
            {
                ticketsQuery = ticketsQuery.Where(t => t.status != null && t.status.ToLower() == ticketStatus.Trim().ToLower());
            }

            if (fromDate.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.issuanceTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                ticketsQuery = ticketsQuery.Where(t => t.issuanceTime <= toDate.Value);
            }

            var ticketItems = ticketsQuery.Select(t => new ServiceTicketItemDto
            {
                id = t.sttId,
                ticketNumber = t.sttId,
                status = t.status,
                stage = t.stage,
                handlerUser = t.Users != null ? t.Users.username : "Chưa xử lý",
                issuanceTime = t.issuanceTime,
                useTime = t.useTime,
                counterName = t.Counter != null ? t.Counter.counterName : "Trống"
            }).ToList();

            // Tổng hợp danh sách quầy tiếp nhận dịch vụ
            var assignedCounters = service.counters.Select(c => new ServiceCounterItemDto
            {
                id = c.id,
                counterId = c.counterId,
                counterName = c.counterName,
                isActive = c.isActive,
                status = c.status
            }).ToList();

            // Nếu quan hệ Nhiều-Nhiều chưa có dữ liệu, lấy bù từ lịch sử quầy đã từng cấp vé
            if (!assignedCounters.Any())
            {
                assignedCounters = service.Stickers
                    .Where(t => t.Counter != null)
                    .Select(t => t.Counter!)
                    .GroupBy(c => c.id)
                    .Select(g => g.First())
                    .Select(c => new ServiceCounterItemDto
                    {
                        id = c.id,
                        counterId = c.counterId,
                        counterName = c.counterName,
                        isActive = c.isActive,
                        status = c.status
                    }).ToList();
            }

            return new ServiceDetailDto
            {
                id = service.id,
                unitId = service.unitId,
                serviceId = service.serviceId,
                serviceName = service.serviceName,
                description = service.description,
                image = service.image,
                isActive = service.isActive,
                sharingLink = service.sharingLink,
                averageTime = service.averageTime,
                totalTicketsIssued = service.Stickers.Count,
                assignedCounters = assignedCounters,
                issuedTickets = ticketItems
            };
        }

        // SVC-02 & SVC-03: Thêm mới dịch vụ
        public async Task<(bool Success, string Message, int? NewId, string? ServiceId)> CreateServiceAsync(CreateServiceDto dto)
        {
            bool unitExists = await _context.units.AnyAsync(u => u.unitId == dto.unitId);
            if (!unitExists)
                return (false, $"Đơn vị có ID = {dto.unitId} không tồn tại trong hệ thống.", null, null);

            bool isDuplicate = await _context.services.AnyAsync(s =>
                s.unitId == dto.unitId && s.serviceId.ToLower() == dto.serviceId.Trim().ToLower());

            if (isDuplicate)
                return (false, $"Mã dịch vụ '{dto.serviceId}' đã tồn tại trong đơn vị.", null, null);

            var service = new Service
            {
                unitId = dto.unitId,
                serviceId = dto.serviceId.Trim(),
                serviceName = dto.serviceName.Trim(),
                description = dto.description,
                image = dto.image,
                isActive = dto.isActive,
                sharingLink = dto.sharingLink,
                averageTime = dto.averageTime
            };

            _context.services.Add(service);
            await _context.SaveChangesAsync();

            return (true, "Thêm dịch vụ thành công.", service.id, service.serviceId);
        }

        // SVC-04: Cập nhật thông tin dịch vụ
        public async Task<(bool Success, string Message)> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            var service = await _context.services.FirstOrDefaultAsync(s => s.id == id);
            if (service == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {id}");

            service.serviceName = dto.serviceName.Trim();
            service.description = dto.description;
            service.isActive = dto.isActive;
            service.sharingLink = dto.sharingLink;
            service.averageTime = dto.averageTime;
            if (dto.image != null)
            {
                service.image = dto.image;
            }

            await _context.SaveChangesAsync();
            return (true, "Cập nhật dịch vụ thành công.");
        }

        // SVC-06: Xóa dịch vụ (Bảo vệ toàn vẹn lịch sử vé)
        public async Task<(bool Success, string Message)> DeleteServiceAsync(int id)
        {
            var service = await _context.services.FirstOrDefaultAsync(s => s.id == id);
            if (service == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {id}");

            bool hasTickets = await _context.stickers.AnyAsync(t => t.serviceId == service.id);
            bool hasBookings = await _context.bookings.AnyAsync(b => b.serviceId == service.id);

            if (hasTickets || hasBookings)
                return (false, "Không thể xóa dịch vụ này vì đã có phiếu STT hoặc lịch hẹn phát sinh. Hãy chuyển trạng thái sang 'Ngưng hoạt động'.");

            _context.services.Remove(service);
            await _context.SaveChangesAsync();
            return (true, "Xóa dịch vụ thành công.");
        }
    }
}