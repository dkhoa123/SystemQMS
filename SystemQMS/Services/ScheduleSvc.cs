using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.ViewModels.ScheduleDto;
using SystemQMS.Services.Interface;
using SystemQMS.Constants;
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
        // Use the local wall-clock time because startTime/endTime are TimeSpan values.
        var t = now.TimeOfDay;
        var start = shift.StartTime;
        var end = shift.EndTime;

        return start <= end
            ? t >= start && t <= end
            : t >= start || t <= end;
    }
    public async Task<bool> IsWithinOperatingHoursAsync(int serviceId, CancellationToken ct = default)
    {
        var shifts = await _context.Shedules
            .Where(x => x.ServiceId == serviceId)
            .ToListAsync(ct);
        if (shifts.Count == 0)
            return true;
        var now = DateTimeOffset.Now;
        return shifts.Any(x => IsInShift(now, x));
    }
    public async Task<ServiceAvailabilityResponse> CheckServiceAvailabilityAsync(int serviceId, CancellationToken ct = default)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId, ct);
        if (service == null)
            throw new KeyNotFoundException("Dịch vụ không tồn tại");

        var now = DateTimeOffset.Now;
        var shifts = await _context.Shedules
            .Where(x => x.ServiceId == serviceId)
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct);

        var current = shifts.FirstOrDefault(x => IsInShift(now, x));
        bool inHours = shifts.Count == 0 || current != null;

        // Còn ít nhất 1 quầy đang mở (Idle/Serving, isActive) phục vụ dịch vụ này không
        bool hasOpenCounter = await _context.Counters.AnyAsync(c =>
            c.IsActive
            && (c.Status == CounterStatus.Idle || c.Status == CounterStatus.Serving)
            && c.Services.Any(s => s.Id == serviceId), ct);


        var result = new ServiceAvailabilityResponse
        {
            serviceId = service.Id,
            serviceName = service.ServiceName,
            inWorkingHours = inHours,
            hasOpenCounter = hasOpenCounter,
            currentShift = current?.WordShift,
            shifts = shifts.Select(x => new ShiftDto
            {
                wordShift = x.WordShift,
                startTime = x.StartTime.ToString(@"hh\:mm"),
                endTime = x.EndTime.ToString(@"hh\:mm")
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

