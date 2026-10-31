using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.ViewModels.ScheduleDto;
using SystemQMS.Services.Interface;
namespace SystemQMS.Services;

public class ScheduleSvc : IScheduleSvc
{
    private readonly MyDbContext _context;
    private readonly ILogger<ScheduleSvc> _logger;

    public ScheduleSvc(MyDbContext context, ILogger<ScheduleSvc> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra thời điểm 'now' có nằm trong ca làm việc không.
    /// Hỗ trợ cả ca xuyên đêm (end < start).
    /// </summary>
    /// <remarks>
    /// LƯU Ý QUAN TRỌNG: 
    /// 1. Npgsql trả 'timestamptz' về DateTimeOffset ở múi giờ UTC (làm mất offset gốc +07:00).
    ///    Bắt buộc phải quy đổi cả 2 vế về .ToUniversalTime() trước khi so sánh .TimeOfDay,
    ///    nếu không ca làm việc sẽ bị lệch giờ sai hoàn toàn.
    /// 2. Dùng toán tử 3 ngôi để phân biệt:
    ///    - start <= end: Ca trong ngày (t nằm giữa start và end).
    ///    - start > end: Ca xuyên đêm (t >= start hoặc t <= end).
    /// </remarks>
    private static bool IsInShift(DateTimeOffset now, SheduleActive shift)
    {
        var t = now.ToUniversalTime().TimeOfDay;
        var start = shift.startTime.ToUniversalTime().TimeOfDay;
        var end = shift.endTime.ToUniversalTime().TimeOfDay;

        return start <= end
            ? t >= start && t <= end
            : t >= start || t <= end;
    }
    public async Task<bool> IsWithinOperatingHoursAsync(int serviceId, CancellationToken ct = default)
    {
        var shifts = await _context.shedules
            .Where(x => x.serviceId == serviceId)
            .ToListAsync(ct);
        if (shifts.Count == 0)
            return true;
        var now = DateTimeOffset.Now;
        return shifts.Any(x => IsInShift(now, x));

        
    }
    public async Task<ServiceAvailabilityResponse> CheckServiceAvailabilityAsync(int serviceId, CancellationToken ct = default)
    {
        var service = await _context.services.FirstOrDefaultAsync(s => s.id == serviceId, ct);
        if (service == null)
            throw new KeyNotFoundException("Dịch vụ không tồn tại");

        var now = DateTimeOffset.Now;
        var shifts = await _context.shedules
            .Where(x => x.serviceId == serviceId)
            .OrderBy(x => x.startTime)
            .ToListAsync(ct);

        var current = shifts.FirstOrDefault(x => IsInShift(now, x));
        bool inHours = shifts.Count == 0 || current != null;

        // Còn ít nhất 1 quầy đang mở (Idle/Serving, isActive) phục vụ dịch vụ này không
        bool hasOpenCounter = await _context.counters.AnyAsync(c =>
            c.isActive
            && (c.status == CounterStatus.Idle || c.status == CounterStatus.Serving)
            && c.Services.Any(s => s.id == serviceId), ct);


        var result = new ServiceAvailabilityResponse
        {
            serviceId = service.id,
            serviceName = service.serviceName,
            inWorkingHours = inHours,
            hasOpenCounter = hasOpenCounter,
            currentShift = current?.wordShift,
            shifts = shifts.Select(x => new ShiftDto
            {
                wordShift = x.wordShift,
                // Hiển thị theo giờ địa phương của server (cấu hình lưu kèm +07:00)
                startTime = x.startTime.ToLocalTime().ToString("HH:mm"),
                endTime = x.endTime.ToLocalTime().ToString("HH:mm")
            }).ToList()
        };

        // Ngoài giờ luôn disable, không phụ thuộc quầy; lý do ngoài giờ ưu tiên trước (SRS 3.12)
        if (!inHours)
        {
            result.isAvailable = false;
            result.reason = "OutOfHours";
        }
        else if (!hasOpenCounter)
        {
            result.isAvailable = false;
            result.reason = "NoOpenCounter";
        }
        else
        {
            result.isAvailable = true;
            result.reason = null;
        }
        return result;
    }

}