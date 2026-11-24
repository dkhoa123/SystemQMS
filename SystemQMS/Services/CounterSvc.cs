using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.RegularExpressions;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;
using SystemQMS.ViewModels.DspDto;
using SystemQMS.ViewModels.PageResultDto;

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

        // CNT-01: danh sách quầy, lọc theo trạng thái + tìm kiếm
        public PageResultDto<CounterResponse> GetCountersAsync(FilterCounterDto filter)
        {
            var query = _context.Counters
                .Include(c => c.Services)
                .Include(d => d.Devices)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.keyword))
            {
                query = query.Where(c => c.CounterName.Contains(filter.keyword) || c.Devices.Any(d => d.DeviceName.Contains(filter.keyword)));
            }

            if(!string.IsNullOrEmpty(filter.serviceName))
            {
                query = query.Where(c => c.Services.Any(s => s.ServiceName == filter.serviceName));
            }

            if(!string.IsNullOrEmpty(filter.use))
            {
                if (filter.use == "IN_USE")
                {
                    query = query.Where(c => c.Services.Any());
                }
                else if (filter.use == "FREE")
                {
                    query = query.Where(c => !c.Services.Any());
                }
            }

            // Đếm tổng số bản ghi sau khi lọc
            int totalRecords = query.Count();

            int page = Math.Max(1, filter.page); // Đảm bảo trang tối thiểu là 1
            int pageSize = Math.Max(1, filter.pageSize); // Đảm bảo kích thước trang tối thiểu là 1

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / filter.pageSize
            );

            // Lấy dữ liệu theo trang
            var counters = query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = counters.Select(c => new CounterResponse
            {
                id = c.Id.ToString(),
                name = c.CounterName,

                deviceCode = c.Devices.Any()
                        ? c.Devices.First().DeviceId
                        : null,

                inUse = c.Status,

                services = c.Services
                     .Select(s => s.ServiceName)
                     .ToList()

            }).ToList();

            return new PageResultDto<CounterResponse>
            {
                items = data,
                page = page,
                pageSize = pageSize,
                total = totalPages
            };
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