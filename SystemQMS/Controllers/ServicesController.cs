using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemQMS.DTOs.Service;
using SystemQMS.Models;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ServicesController(MyDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // SVC-01: Danh sách dịch vụ (Lọc theo quầy xử lý, trạng thái, tìm mã/tên)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetServices(
            [FromQuery] int? unitId,
            [FromQuery] int? counterId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search)
        {
            var query = _context.services
                .AsNoTracking()
                .Include(s => s.stickers)
                .AsQueryable();

            if (unitId.HasValue)
            {
                query = query.Where(s => s.unitId == unitId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.isActive == isActive.Value);
            }

            if (counterId.HasValue)
            {
                query = query.Where(s => s.stickers.Any(st => st.counterId == counterId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(s => s.serviceId.ToLower().Contains(keyword) || 
                                         s.serviceName.ToLower().Contains(keyword));
            }

            var result = await query.Select(s => new ServiceResponseDto
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
                totalTicketsIssued = s.stickers.Count
            }).ToListAsync();

            return Ok(result);
        }

        // =========================================================================
        // SVC-05: Xem chi tiết dịch vụ (Bao gồm quầy xử lý và danh sách STT đã cấp)
        // =========================================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceById(
            int id,
            [FromQuery] string? ticketStatus,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            var service = await _context.services
                .AsNoTracking()
                .Include(s => s.stickers)
                    .ThenInclude(st => st.counter)
                .Include(s => s.stickers)
                    .ThenInclude(st => st.users)
                .FirstOrDefaultAsync(s => s.id == id);

            if (service == null)
            {
                return NotFound(new { message = $"Không tìm thấy dịch vụ có ID = {id}" });
            }

            // Lọc danh sách STT theo trạng thái và thời gian cấp (SVC-05)
            var ticketsQuery = service.stickers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketStatus))
            {
                ticketsQuery = ticketsQuery.Where(t => t.status.ToLower() == ticketStatus.Trim().ToLower());
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
                handlerUser = t.users != null ? t.users.username : "Chưa xử lý",
                issuanceTime = t.issuanceTime,
                useTime = t.useTime,
                counterName = t.counter != null ? t.counter.counterName : "Trống"
            }).ToList();

            // Lấy danh sách các quầy đã phục vụ dịch vụ này
            var assignedCounters = service.stickers
                .Where(t => t.counter != null)
                .Select(t => t.counter!)
                .GroupBy(c => c.id)
                .Select(g => g.First())
                .Select(c => new ServiceCounterItemDto
                {
                    id = c.id,
                    counterId = c.counterId,
                    counterName = c.counterName,
                    isActive = c.isActive,
                    status = c.Status
                }).ToList();

            var detailDto = new ServiceDetailDto
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
                totalTicketsIssued = service.stickers.Count,
                assignedCounters = assignedCounters,
                issuedTickets = ticketItems
            };

            return Ok(detailDto);
        }

        // =========================================================================
        // SVC-02 & SVC-03: Thêm mới dịch vụ, thiết lập chế độ chia quầy & thời gian xử lý
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra trùng mã dịch vụ trong cùng đơn vị Phường
            bool isDuplicate = await _context.services.AnyAsync(s => 
                s.unitId == dto.unitId && s.serviceId.ToLower() == dto.serviceId.Trim().ToLower());

            if (isDuplicate)
            {
                return BadRequest(new { message = $"Mã dịch vụ '{dto.serviceId}' đã tồn tại trong đơn vị." });
            }

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

            return CreatedAtAction(nameof(GetServiceById), new { id = service.id }, new 
            { 
                id = service.id, 
                serviceId = service.serviceId, 
                message = "Thêm dịch vụ thành công." 
            });
        }

        // =========================================================================
        // SVC-04: Cập nhật dịch vụ (Sửa toàn bộ trừ mã dịch vụ serviceId)
        // =========================================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var service = await _context.services.FirstOrDefaultAsync(s => s.id == id);
            if (service == null)
            {
                return NotFound(new { message = $"Không tìm thấy dịch vụ có ID = {id}" });
            }

            // Cập nhật các trường được phép, giữ nguyên serviceId theo quy tắc SVC-04
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

            return Ok(new { message = "Cập nhật dịch vụ thành công." });
        }

        // =========================================================================
        // SVC-06: Xóa dịch vụ (Kiểm tra ràng buộc STT trước khi xóa)
        // =========================================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.services
                .Include(s => s.stickers)
                .FirstOrDefaultAsync(s => s.id == id);

            if (service == null)
            {
                return NotFound(new { message = $"Không tìm thấy dịch vụ có ID = {id}" });
            }

            // Chặn xóa nếu dịch vụ đã có STT (bảng stickers) được cấp
            if (service.stickers.Any())
            {
                return BadRequest(new 
                { 
                    message = "Không thể xóa dịch vụ này vì đã có phiếu STT phát sinh. Vui lòng chuyển trạng thái hoạt động sang 'Ngưng hoạt động'." 
                });
            }

            _context.services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa dịch vụ thành công." });
        }
    }
}