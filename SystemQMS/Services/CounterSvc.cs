using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.RegularExpressions;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Services
{
    public class CounterSvc : ICounterSvc
    {
        private readonly MyDbContext _context;
        private readonly ILogger<CounterSvc> _logger;

        public CounterSvc(MyDbContext context, ILogger<CounterSvc> logger)
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
        public async Task<List<CounterResponse>> GetCountersAsync(
            int unitId, string? status = null, bool? isActive = null, string? search = null)
        {
            var query = _context.counters.AsNoTracking().Where(c => c.unitId == unitId);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.status == status);
            if (isActive.HasValue)
                query = query.Where(c => c.isActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                // Postgres LIKE phân biệt hoa/thường -> dùng ILike để tìm không phân biệt
                var term = $"%{search.Trim()}%";
                query = query.Where(c => EF.Functions.ILike(c.counterId, term)
                    || (c.counterName != null && EF.Functions.ILike(c.counterName, term)));
            }

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
        public async Task<CounterResponse> CreateCounterAsync(CreateCounterRequest request)
        {
            if (!Regex.IsMatch(request.counterId ?? "", @"^[A-Za-z0-9_-]+$"))
                throw new ArgumentException("Mã quầy chỉ gồm chữ, số, '-' và '_'");
            if (request.counterName != null &&
                !Regex.IsMatch(request.counterName, @"^[\p{L}0-9 _-]+$"))
                throw new ArgumentException("Tên quầy chứa kí tự không hợp lệ");

            bool exists = await _context.counters.AnyAsync(c =>
                c.unitId == request.unitId && c.counterId == request.counterId);
            if (exists)
                throw new ArgumentException("Mã quầy đã tồn tại trong đơn vị");

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.services
                .Where(s => serviceIds.Contains(s.id) && s.unitId == request.unitId)
                .ToListAsync();

            if (!await _context.units.AnyAsync(u => u.unitId == request.unitId))
                throw new ArgumentException("Đơn vị không tồn tại");

            if (services.Count != serviceIds.Distinct().Count())
                throw new ArgumentException("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");

            var counter = new Counter
            {
                counterId = (request.counterId ?? "").Trim(),
                unitId = request.unitId,
                counterName = request.counterName?.Trim(),
                description = request.description,
                isActive = request.isActive,
                status = CounterStatus.Idle,
                Services = services
            };
            _context.counters.Add(counter);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "IX_counters_unitId_counterId"
                })
            {
                // Chống race: 2 request cùng mã vượt qua AnyAsync ở trên.
                // Lọc theo tên constraint để lỗi unique khác (vd. PK) không bị báo nhầm thành trùng mã.
                _logger.LogWarning(ex, "Tạo quầy trùng mã: UnitId={UnitId}, CounterId={CounterId}",
                    request.unitId, counter.counterId);
                throw new ArgumentException("Mã quầy đã tồn tại trong đơn vị");
            }

            return new CounterResponse
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
            };
        }

        // CNT-03: cập nhập quầy - sửa tất cả trừ mã quầy, chặn khi đang Serving
        public async Task<CounterResponse> UpdateCounterAsync(int id, UpdateCounterRequest request)
        {
            var counter = await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id);

            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            if (counter.status == CounterStatus.Serving)
                throw new ArgumentException("Quầy đang có người xử lý, không thể cập nhật");

            if (request.counterName != null && !Regex.IsMatch(request.counterName, @"^[\p{L}0-9 _-]+$"))
                throw new ArgumentException("Tên quầy chứa kí tự không hợp lệ");

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.services
                .Where(s => serviceIds.Contains(s.id) && s.unitId == counter.unitId)
                .ToListAsync();

            if (services.Count != serviceIds.Distinct().Count())
                throw new ArgumentException("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");

            counter.counterName = request.counterName?.Trim();
            counter.description = request.description;
            counter.isActive = request.isActive;
            counter.Services.Clear();
            foreach (var s in services) counter.Services.Add(s);
            await _context.SaveChangesAsync();
            return ToResponse(counter);
        }

        //CNT-04a: chi tiết quầy + người xử lý
        public async Task<CounterDetailResponse> GetCounterAsync(int id)
        {
            var counter = await _context.counters
                .AsNoTracking()
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            var handlers = await _context.users.AsNoTracking()
                .Where(u => u.counterId == id && u.isActive && u.username != null)
                .Select(u => u.username!)
                .ToListAsync();

            var baseDto = ToResponse(counter);
            return new CounterDetailResponse
            {
                id = baseDto.id,
                counterId = baseDto.counterId,
                unitId = baseDto.unitId,
                counterName = baseDto.counterName,
                description = baseDto.description,
                isActive = baseDto.isActive,
                Status = baseDto.Status,
                services = baseDto.services,
                handlerNames = handlers
            };
        }

        //CNT-04b: danh sách STT của quầy
        public async Task<List<CounterStickerResponse>> GetCounterStickersAsync(
            int id, string? status = null, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            if (!await _context.counters.AnyAsync(c => c.id == id))
                throw new KeyNotFoundException("Quầy không tồn tại");

            var query = _context.stickers.AsNoTracking().Where(s => s.counterId == id);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.status == status);
            if (from.HasValue) query = query.Where(s => s.issuanceTime >= from.Value);
            if (to.HasValue) query = query.Where(s => s.issuanceTime <= to.Value);

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

        // CNT-05 
        public async Task DeleteCounterAsync(int id, CancellationToken ct = default)
        {
            var counter = await _context.counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.id == id, ct);
            if (counter == null)
            {
                _logger.LogWarning("Xóa quầy thất bại: không tìm thấy CounterId={CounterId}", id);
                throw new KeyNotFoundException("Quầy không tồn tại");
            }

            if (await _context.stickers.AnyAsync(s => s.counterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có phiếu gọi số", id);
                throw new ArgumentException("Quầy đã có phiếu gọi số, không thể xóa");
            }

            if (await _context.users.AnyAsync(u => u.counterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có cán bộ được gán", id);
                throw new ArgumentException("Quầy đã có cán bộ được gán, không thể xóa");
            }

            _context.counters.Remove(counter);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
            {
                // Chỉ bắt lỗi FK (23503); lỗi DB khác phải nổi lên thành 500 để còn thấy log
                _logger.LogWarning(ex, "Xóa quầy thất bại: CounterId={CounterId} vừa phát sinh dữ liệu tham chiếu", id);
                throw new ArgumentException("Quầy vừa phát sinh dữ liệu liên quan, không thể xóa");
            }

            _logger.LogInformation("Đã xóa quầy: CounterId={CounterId}, UnitId={UnitId}", id, counter.unitId);
        }
    }
}