using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.RegularExpressions;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DspDto;

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
                id = counter.Id,
                counterId = counter.CounterId,
                unitId = counter.UnitId,
                counterName = counter.CounterName,
                description = counter.Description,
                isActive = counter.IsActive,
                Status = counter.Status,
                services = counter.Services.Select(s => new ServiceInCounterDto
                {
                    id = s.Id,
                    serviceId = s.ServiceId,
                    serviceName = s.ServiceName
                }).ToList()
            };
        }

        // CNT-01: danh sách quầy, lọc theo trạng thái + tìm kiếm
        public async Task<List<CounterResponse>> GetCountersAsync(
            int unitId, string? status = null, bool? isActive = null, string? search = null)
        {
            var query = _context.Counters.AsNoTracking().Where(c => c.UnitId == unitId);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);
            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                // Postgres LIKE phân biệt hoa/thường -> dùng ILike để tìm không phân biệt
                var term = $"%{search.Trim()}%";
                query = query.Where(c => EF.Functions.ILike(c.CounterId, term)
                    || (c.CounterName != null && EF.Functions.ILike(c.CounterName, term)));
            }

            return await query
                .OrderBy(c => c.Id)
                .Select(c => new CounterResponse
                {
                    id = c.Id,
                    counterId = c.CounterId,
                    unitId = c.UnitId,
                    counterName = c.CounterName,
                    description = c.Description,
                    isActive = c.IsActive,
                    Status = c.Status,
                    services = c.Services.Select(s => new ServiceInCounterDto
                    {
                        id = s.Id,
                        serviceId = s.ServiceId,
                        serviceName = s.ServiceName
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

            bool exists = await _context.Counters.AnyAsync(c =>
                c.UnitId == request.unitId && c.CounterId == request.counterId);
            if (exists)
                throw new ArgumentException("Mã quầy đã tồn tại trong đơn vị");

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.Id) && s.UnitId == request.unitId)
                .ToListAsync();

            if (!await _context.Units.AnyAsync(u => u.UnitId == request.unitId))
                throw new ArgumentException("Đơn vị không tồn tại");

            if (services.Count != serviceIds.Distinct().Count())
                throw new ArgumentException("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");

            var counter = new Counter
            {
                CounterId = (request.counterId ?? "").Trim(),
                UnitId = request.unitId,
                CounterName = request.counterName?.Trim(),
                Description = request.description,
                IsActive = request.isActive,
                Status = CounterStatus.Idle,
                Services = services
            };
            _context.Counters.Add(counter);

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
                    request.unitId, counter.CounterId);
                throw new ArgumentException("Mã quầy đã tồn tại trong đơn vị");
            }

            return new CounterResponse
            {
                id = counter.Id,
                counterId = counter.CounterId,
                unitId = counter.UnitId,
                counterName = counter.CounterName,
                description = counter.Description,
                isActive = counter.IsActive,
                Status = counter.Status,
                services = services.Select(s => new ServiceInCounterDto
                {
                    id = s.Id,
                    serviceId = s.ServiceId,
                    serviceName = s.ServiceName
                }).ToList()
            };
        }

        // CNT-03: cập nhập quầy - sửa tất cả trừ mã quầy, chặn khi đang Serving
        public async Task<CounterResponse> UpdateCounterAsync(int id, UpdateCounterRequest request)
        {
            var counter = await _context.Counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            if (counter.Status == CounterStatus.Serving)
                throw new ArgumentException("Quầy đang có người xử lý, không thể cập nhật");

            if (request.counterName != null && !Regex.IsMatch(request.counterName, @"^[\p{L}0-9 _-]+$"))
                throw new ArgumentException("Tên quầy chứa kí tự không hợp lệ");

            var serviceIds = request.serviceIds ?? new List<int>();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.Id) && s.UnitId == counter.UnitId)
                .ToListAsync();

            if (services.Count != serviceIds.Distinct().Count())
                throw new ArgumentException("Có dịch vụ không tồn tại hoặc không thuộc đơn vị");

            counter.CounterName = request.counterName?.Trim();
            counter.Description = request.description;
            counter.IsActive = request.isActive;
            counter.Services.Clear();
            foreach (var s in services) counter.Services.Add(s);
            await _context.SaveChangesAsync();
            return ToResponse(counter);
        }

        //CNT-04a: chi tiết quầy + người xử lý
        public async Task<CounterDetailResponse> GetCounterAsync(int id)
        {
            var counter = await _context.Counters
                .AsNoTracking()
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            var handlers = await _context.Users.AsNoTracking()
                .Where(u => u.CounterId == id && u.IsActive && u.Username != null)
                .Select(u => u.Username!)
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
            if (!await _context.Counters.AnyAsync(c => c.Id == id))
                throw new KeyNotFoundException("Quầy không tồn tại");

            var query = _context.Tickets.AsNoTracking().Where(s => s.CounterId == id);
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.Status == status);
            if (from.HasValue) query = query.Where(s => s.IssuanceTime >= from.Value);
            if (to.HasValue) query = query.Where(s => s.IssuanceTime <= to.Value);

            return await query
                .OrderByDescending(s => s.IssuanceTime)
                .Select(s => new CounterStickerResponse
                {
                    sttId = s.SttId,
                    status = s.Status,
                    stage = s.Stage,
                    issuanceTime = s.IssuanceTime,
                    useTime = s.UseTime,
                    serviceName = s.Service != null ? s.Service.ServiceName : null,
                    handlerName = s.Users != null ? s.Users.Username : null
                }).ToListAsync();
        }

        // CNT-05 
        public async Task DeleteCounterAsync(int id, CancellationToken ct = default)
        {
            var counter = await _context.Counters
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
            if (counter == null)
            {
                _logger.LogWarning("Xóa quầy thất bại: không tìm thấy CounterId={CounterId}", id);
                throw new KeyNotFoundException("Quầy không tồn tại");
            }

            if (await _context.Tickets.AnyAsync(s => s.CounterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có phiếu gọi số", id);
                throw new ArgumentException("Quầy đã có phiếu gọi số, không thể xóa");
            }

            if (await _context.Users.AnyAsync(u => u.CounterId == id, ct))
            {
                _logger.LogWarning("Xóa quầy thất bại: CounterId={CounterId} đã có cán bộ được gán", id);
                throw new ArgumentException("Quầy đã có cán bộ được gán, không thể xóa");
            }

            _context.Counters.Remove(counter);
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

            _logger.LogInformation("Đã xóa quầy: CounterId={CounterId}, UnitId={UnitId}", id, counter.UnitId);
        }
    }
}