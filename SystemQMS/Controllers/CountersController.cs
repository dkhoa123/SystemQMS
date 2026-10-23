using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using SystemQMS.Dtos;
using SystemQMS.Models;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountersController : ControllerBase
    {
        private readonly MyDbContext _context;
        public CountersController(MyDbContext context)
        {
            _context = context;
        }

        // CNT-01: danh sách quầy, lọc theo trạng thái + tìm kiếm
        [HttpGet]
        public async Task<ActionResult<List<CounterResponse>>> GetCounters(int unitId, string? status = null, bool? isActive = null, string? search = null)
        {

            var query = _context.counters.Where(c => c.unitId == unitId);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.status == status);
            if (isActive.HasValue)
                query = query.Where(c => c.isActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.counterId.Contains(search)
                    || (c.counterName != null && c.counterName.Contains(search)));

            return await query
                .OrderBy(c => c.id)
                .Select(c => new CounterResponse
            {
                id = c.id,
                counterId = c.counterId,
                unitId = c.unitId,
                counterName = c.counterName,
                description = c.description,
                isActive = c.isActive,
                Status = c.status,
                services = c.Services.Select(s => new ServiceInCounterDto
                {
                    id = s.id,
                    serviceId = s.serviceId,
                    serviceName = s.serviceName
                }).ToList()
            }).ToListAsync();
        }

        // CNT-02: thêm quầy
        [HttpPost]
        public async Task<ActionResult<CounterResponse>> CreateCounter(CreateCounterRequest request)
        {
            if (!Regex.IsMatch(request.counterId ?? "", @"^[A-Za-z0-9_-]+$"))
                return BadRequest("Mã quầy chỉ gồm chữ, số, '-' và '_'");
            if (request.counterName != null &&
                !Regex.IsMatch(request.counterName, @"^[\p{L}0-9 _-]+$"))
                return BadRequest("Tên quầy chứa kí tự không hợp lệ");

            bool exists = await _context.counters.AnyAsync(c =>
                c.unitId == request.unitId && c.counterId == request.counterId);
            if (exists)
                return BadRequest("Mã quầy đã tồn tại trong đơn vị");

            var services = await _context.services
                .Where(s => request.serviceIds.Contains(s.id) && s.unitId == request.unitId)
                .ToListAsync();

            if (!await _context.units.AnyAsync(u => u.unitId == request.unitId))
                return BadRequest(new { message = "Đơn vị không tồn tại" });

            if (services.Count != request.serviceIds.Distinct().Count())
                return BadRequest("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");

            var counter = new Counter
            {
                counterId = request.counterId,
                unitId = request.unitId,
                counterName = request.counterName,
                description = request.description,
                isActive = request.isActive,
                status = CounterStatus.Idle,
                Services = services
            };
            _context.counters.Add(counter);
            await _context.SaveChangesAsync();

            return Created($"/api/counters/{counter.id}", new CounterResponse
            {
                id = counter.id,
                counterId = counter.counterId,
                unitId = counter.unitId,
                counterName = counter.counterName,
                description = counter.description,
                isActive = counter.isActive,
                Status = counter.status,
                services = services.Select(s => new ServiceInCounterDto
                {
                    id = s.id,
                    serviceId = s.serviceId,
                    serviceName = s.serviceName
                }).ToList()
            });
        }

    }
}
