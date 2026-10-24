using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.RegularExpressions;
using SystemQMS.Dtos;
using SystemQMS.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CountersController> _logger;
        public CountersController(MyDbContext context,ILogger<CountersController> logger)
        {
            _context = context;
            _logger = logger;
        }
        private CounterResponse ToResponse(Counter counter)
        {
            return new CounterResponse
            {
                id = counter.id,
                counterId = counter.counterId,
                unitId = counter.unitId,
                counterName = counter.counterName,
                description = counter.description,
                isActive = counter.isActive,
                Status = counter.status,
                services = counter.Services.Select(s => new ServiceInCounterDto
                {
                    id = s.id,
                    serviceId = s.serviceId,
                    serviceName = s.serviceName
                }).ToList()
            };
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

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.services
                .Where(s => serviceIds.Contains(s.id) && s.unitId == request.unitId)
                .ToListAsync();

            if (!await _context.units.AnyAsync(u => u.unitId == request.unitId))
                return BadRequest(new { message = "Đơn vị không tồn tại" });

            if (services.Count != serviceIds.Distinct().Count())
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
        // CNT-03: cập nhập quầy - sửa tất cả trừ mã quầy, chặn khi đang Serving
        [HttpPut("{id}")]
        public async Task<ActionResult<CounterResponse>> UpdateCounter(UpdateCounterRequest request, int id)
        {
            var counter = await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id);

            if (counter == null)
                return NotFound(new { message = "Quầy không tồn tại" });

            if (counter.status == CounterStatus.Serving)
                return BadRequest("Quầy đang có người xử lý, không thể cập nhật");

            if (request.counterName != null && !Regex.IsMatch(request.counterName, @"^[\p{L}0-9 _-]+$"))
                return BadRequest("Tên quầy chứa kí tự không hợp lệ");

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.services
                .Where(s => serviceIds.Contains(s.id) && s.unitId == counter.unitId)
                .ToListAsync();

            if (services.Count != serviceIds.Distinct().Count())
                return BadRequest("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");
            counter.counterName = request.counterName;
            counter.description = request.description;
            counter.isActive = request.isActive;
            counter.Services.Clear();
            foreach (var s in services) counter.Services.Add(s);
            await _context.SaveChangesAsync();
            return Ok(ToResponse(counter));
        }


        //CNT-04

        //CNT-04a: chi tiết quầy + người xử lý
        [HttpGet("{id}")]
        public async Task<ActionResult<CounterDetailResponse>> GetCounter(int id)
        {
            var counter = await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id);
            if (counter == null) return NotFound();

            var handlers = await _context.users
                .Where(u => u.counterId == id && u.username != null)
                .Select(u => u.username!)
                .ToListAsync();
            var result = new CounterDetailResponse
            {
                handlerNames = handlers
            };

            var baseDto = ToResponse(counter);
            result.id = baseDto.id; result.counterId = baseDto.counterId;
            result.unitId = baseDto.unitId; result.counterName = baseDto.counterName;
            result.description = baseDto.description; result.isActive = baseDto.isActive;
            result.Status = baseDto.Status; result.services = baseDto.services;
            return Ok(result);
        }

        //CNT-04b: danh sách STT của quầy
        [HttpGet("{id}/stickers")]
        public async Task<ActionResult<List<CounterStickerResponse>>> GetCounterStickers(
    int id, string? status = null, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {

            var query = _context.stickers.Where(s => s.counterId == id);
            if (!await _context.counters.AnyAsync(c => c.id == id))
                return NotFound();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.status == status);
            if (from.HasValue) query = query.Where(s => s.issuanceTime >= from.Value);
            if(to.HasValue) query = query.Where(s => s.issuanceTime <= to.Value);
            return await query
                .OrderByDescending(s => s.issuanceTime)
                .Select(s => new CounterStickerResponse
            {
                sttId = s.sttId,
                status = s.status,
                stage = s.stage,
                issuanceTime = s.issuanceTime,
                useTime = s.useTime,
                serviceName = s.Service != null ? s.Service.serviceName : null,
                handlerName = s.Users != null ? s.Users.username : null
            }).ToListAsync();
        }
        //CNT-05: xoá quầy (do chưa biết yc nên thêm điều kiện xoá)
        
        [HttpDelete("{id}")]
        public async Task<ActionResult>DeleteCounter(int id, CancellationToken ct)
        {
            var counter = await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id);
            if (counter == null)
            {
                _logger.LogWarning("Xóa quầy thất bại: không tìm thấy CounterId={CounterId}", id);
                return NotFound(new { message = "Quầy không tồn tại" });
            }

            if (await _context.stickers.AnyAsync(s => s.counterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có phiếu gọi số", id);
                return BadRequest(new { message = "Quầy đã có phiếu gọi số, không thể xóa" });
            }

            if (await _context.users.AnyAsync(u => u.counterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có cán bộ được gán", id);
                return BadRequest(new { message = "Quầy đã có cán bộ được gán, không thể xóa" });
            }

            
            _context.counters.Remove(counter);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} vừa phát sinh dữ liệu tham chiếu", id);
                return BadRequest(new { message = "Quầy vừa phát sinh dữ liệu liên quan, không thể xóa" });
            }


            _logger.LogInformation("Đã xóa quầy: CounterId={CounterId}, UnitId={UnitId}", id, counter.unitId);
            return NoContent(); // 204 
        }

    }
}
