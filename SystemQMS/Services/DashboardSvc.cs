using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DashboardDto;
using static SystemQMS.ViewModels.DashboardDto.DashboardDto;

namespace SystemQMS.Services
{
    public class DashboardSvc : IDashboardSvc
    {
        private MyDbContext _context;
        private TimeZoneInfo _timeZone;
        public DashboardSvc(MyDbContext context)
        {
            _context = context;

            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                    "SE Asia Standard Time"
                );
            }
            catch (TimeZoneNotFoundException)
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                    "Asia/Ho_Chi_Minh"
                );
            }
        }

        // Mốc ngày/giờ tính theo múi giờ đơn vị (SRS 6.2). Mặc định UTC+7.
        private static readonly TimeSpan Vn = TimeSpan.FromHours(7);

        // Thang điểm giả định 1-5, "đánh giá cao" khi trung bình >= 4. Chốt lại với BA.
        private const double HighRatingThreshold = 4.0;
        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------
        private static DateTimeOffset StartOfDay(DateTimeOffset t)
        {
            var l = t.ToOffset(Vn);
            return new DateTimeOffset(l.Year, l.Month, l.Day, 0, 0, 0, Vn);
        }

        private static (DateTimeOffset from, DateTimeOffset to)
         ResolveRange(DashboardFilterRequest f)
        {
            var now = DateTimeOffset.UtcNow;

            var todayStart = StartOfDay(now);
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);

            var from = (f.From ?? todayStart).ToUniversalTime();
            var to = (f.To ?? todayEnd).ToUniversalTime();

            return (from, to);
        }

        private static DateTimeOffset Bucket(DateTimeOffset t, DashboardGranularity g)
        {
            var l = t.ToOffset(Vn);
            return g switch
            {
                DashboardGranularity.Hour => new DateTimeOffset(l.Year, l.Month, l.Day, l.Hour, 0, 0, Vn),
                DashboardGranularity.Month => new DateTimeOffset(l.Year, l.Month, 1, 0, 0, 0, Vn),
                DashboardGranularity.Year => new DateTimeOffset(l.Year, 1, 1, 0, 0, 0, Vn),
                _ => new DateTimeOffset(l.Year, l.Month, l.Day, 0, 0, 0, Vn),
            };
        }

        // ----------------------------------------------------------------
        // DSH-01
        // ----------------------------------------------------------------

        private (DateOnly from, DateOnly to) GetDefaultDateRange(StatisticsFilter filter)
        {
            // Lấy ngày hiện tại theo múi giờ Việt Nam
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
            DateOnly today = DateOnly.FromDateTime(localNow);

            switch (filter)
            {
                case StatisticsFilter.Gio:
                    // Hôm nay (từ 00:00 đến 23:59 hôm nay -> trả về 24 cột giờ)
                    return (today, today);

                case StatisticsFilter.Ngay:
                    // Tuần hiện tại: từ Thứ 2 đầu tuần đến Chủ Nhật cuối tuần (trả về 7 ngày T2 -> CN)
                    int daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
                    DateOnly startOfWeek = today.AddDays(-daysFromMonday);
                    DateOnly endOfWeek = startOfWeek.AddDays(6);
                    return (startOfWeek, endOfWeek);

                case StatisticsFilter.Tuan:
                    // Tháng hiện tại: từ ngày 1 đến ngày cuối tháng (trả về 4-5 tuần)
                    DateOnly startOfMonth = new DateOnly(today.Year, today.Month, 1);
                    DateOnly endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    return (startOfMonth, endOfMonth);

                case StatisticsFilter.Thang:
                    // Năm hiện tại: từ 01/01 đến 31/12 (trả về 12 tháng)
                    DateOnly startOfYear = new DateOnly(today.Year, 1, 1);
                    DateOnly endOfYear = new DateOnly(today.Year, 12, 31);
                    return (startOfYear, endOfYear);

                default:
                    throw new ArgumentException("Bộ lọc thống kê không hợp lệ.");
            }
        }
        public async Task<List<TicketSummaryDto>> GetTicketIssuedAsync(
                int unitId,
                StatisticsFilter filter,
                DateOnly? fromDate = null,
                DateOnly? toDate = null)
        {
            // Nếu không truyền fromDate hoặc toDate, tự động lấy theo thời điểm hiện tại
            if (!fromDate.HasValue || !toDate.HasValue)
            {
                var defaultRange = GetDefaultDateRange(filter);
                fromDate ??= defaultRange.from;
                toDate ??= defaultRange.to;
            }

            if (fromDate > toDate)
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            // 1. Chuyển khoảng ngày Việt Nam sang UTC (dùng .Value vì đã đảm bảo có dữ liệu)
            DateTime localStart = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            DateTime localEnd = toDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            if (fromDate > toDate)
            {
                throw new ArgumentException(
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc."
                );
            }

            DateTimeOffset fromUtc = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(localStart, _timeZone),
                TimeSpan.Zero // <-- Ép cứng nhận diện là Offset +00:00
            );

            DateTimeOffset toUtc = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(localEnd, _timeZone),
                TimeSpan.Zero
            );

            // 2. Lấy thời điểm cấp STT từ database
            // Không lọc status vì cần đếm tất cả STT đã cấp

            var issuanceTimes = await _context.Tickets
                .AsNoTracking()
                .Where(s =>
                    s.UnitId == unitId &&
                    s.IssuanceTime >= fromUtc &&
                    s.IssuanceTime < toUtc
                )
                .Select(s => s.IssuanceTime)
                .ToListAsync();

            // 3. Chuyển thời gian UTC sang giờ Việt Nam
            var localTimes = issuanceTimes.Select(time =>
                TimeZoneInfo.ConvertTime(
                    time,
                    _timeZone
                ).DateTime
            ).ToList();

            // 4. Gom nhóm theo filter
            var groupedData = localTimes
                .GroupBy(time => GetBucketStart(time, filter))
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // 5. Xác định mốc bắt đầu và kết thúc
            DateTime firstBucket = GetBucketStart(
                localStart,
                filter
            );

            DateTime lastBucket = GetBucketStart(
                localEnd.AddTicks(-1),
                filter
            );

            // 6. Tạo danh sách kết quả, bao gồm cả mốc bằng 0
            var result = new List<TicketSummaryDto>();

            DateTime current = firstBucket;

            while (current <= lastBucket)
            {
                groupedData.TryGetValue(
                    current,
                    out int total
                );

                result.Add(new TicketSummaryDto
                {
                    time = FormatTime(current, filter, fromDate, toDate),
                    value = total
                });

                current = NextBucket(current, filter);
            }

            return result;
        }

        // Xác định mốc thời gian bắt đầu của một nhóm
        private DateTime GetBucketStart(
            DateTime time,
            StatisticsFilter filter)
        {
            switch (filter)
            {
                case StatisticsFilter.Gio:
                    return new DateTime(
                        time.Year,
                        time.Month,
                        time.Day,
                        time.Hour,
                        0,
                        0
                    );

                case StatisticsFilter.Ngay:
                    return time.Date;

                case StatisticsFilter.Tuan:
                    // Thứ 2 là ngày đầu tuần
                    int daysFromMonday =
                        ((int)time.DayOfWeek + 6) % 7;

                    return time.Date.AddDays(-daysFromMonday);

                case StatisticsFilter.Thang:
                    return new DateTime(
                        time.Year,
                        time.Month,
                        1
                    );

                default:
                    throw new ArgumentException(
                        "Bộ lọc thống kê không hợp lệ."
                    );
            }
        }

        // Xác định mốc tiếp theo
        private DateTime NextBucket(
            DateTime current,
            StatisticsFilter filter)
        {
            return filter switch
            {
                StatisticsFilter.Gio => current.AddHours(1),

                StatisticsFilter.Ngay => current.AddDays(1),

                StatisticsFilter.Tuan => current.AddDays(7),

                StatisticsFilter.Thang => current.AddMonths(1),

                _ => throw new ArgumentException(
                    "Bộ lọc thống kê không hợp lệ."
                )
            };
        }

        // Format nhãn hiển thị trên biểu đồ
        private string FormatTime(
    DateTime time,
    StatisticsFilter filter,
    DateOnly? fromDate,
    DateOnly? toDate)
        {
            return filter switch
            {
                StatisticsFilter.Gio =>
                    fromDate == toDate
                        ? time.ToString("HH:mm")
                        : time.ToString("dd/MM HH:mm"),

                StatisticsFilter.Ngay =>
                    time.ToString("dd/MM"),

                StatisticsFilter.Tuan =>
                        FormatWeekOfMonth(time, fromDate),

                StatisticsFilter.Thang =>
                    time.ToString("MM/yyyy"),

                _ => time.ToString("dd/MM/yyyy")
            };
        }
        private string FormatWeekOfMonth(
    DateTime bucketStart,
    DateOnly? fromDate)
        {
            if (!fromDate.HasValue)
                return $"Tuần {ISOWeek.GetWeekOfYear(bucketStart)}/{bucketStart.Month}";

            // Ngày đầu tiên của tháng được chọn
            var monthStart = new DateTime(
                fromDate.Value.Year,
                fromDate.Value.Month,
                1
            );

            // Tìm thứ Hai đầu tuần chứa ngày đầu tháng
            int offset = ((int)monthStart.DayOfWeek + 6) % 7;
            var firstWeekStart = monthStart.AddDays(-offset);

            // Tính số thứ tự tuần trong tháng
            int weekNumber =
                ((bucketStart.Date - firstWeekStart.Date).Days / 7) + 1;

            return $"Tuần {weekNumber}/{monthStart.Month}";
        }

        // ----------------------------------------------------------------
        // DSH-02
        // ----------------------------------------------------------------
        public async Task<TicketStatusDonutDto> GetTicketStatusAsync()
        {
            var rows = _context.Tickets.AsQueryable();
                

            return new TicketStatusDonutDto
            {
                totalIssued = await rows.CountAsync(s => s.Status == "Completed"),
                totalWaiting = await rows.CountAsync(s => s.Status == "Waiting"),
                totalUsed = await rows.CountAsync(s => s.Status == "Used"),
                totalSkipped = await rows.CountAsync(s => s.Status == "Skipped"),

            };
        }

        // ----------------------------------------------------------------
        // DSH-04
        // ----------------------------------------------------------------
        public async Task<CounterServiceSummaryDto> GetCounterServiceSummaryAsync()
        {
            return new CounterServiceSummaryDto
            {
                totalDevices = await _context.Devices.CountAsync(),
                totalServices = await _context.Services.CountAsync(),
                totalTickets = await _context.Tickets.CountAsync(),
                statusDeviceActive = await _context.Devices.CountAsync(d => d.StatusActive == "AVAILABLE"),
                statusDeviceInactive = await _context.Devices.CountAsync(d => d.StatusActive == "UNAVAILABLE"),
                statusServiceActive = await _context.Services.CountAsync(s => s.IsActive == true),
                statusServiceInactive = await _context.Services.CountAsync(s => s.IsActive == false),
                statusTicketIssued = await _context.Tickets.CountAsync(s => s.Status == "Completed"),
                statusTicketUsed = await _context.Tickets.CountAsync(s => s.Status == "Used"),
                statusTicketSkipped = await _context.Tickets.CountAsync(s => s.Status == "Skipped"),
            };
        }

        // ----------------------------------------------------------------
        // DSH-06: Thống kê STT cấp theo dịch vụ
        // ----------------------------------------------------------------
        public async Task<List<ServiceBarPointDto>> GetTicketsByServiceAsync(
            //int unitId,
            StatisticsFilter filter,
            DateTime fromDate)
        {
            DateTime startDate;
            DateTime endDate;

            // 1. Xác định khoảng thời gian theo giờ Việt Nam
            switch (filter)
            {
                case StatisticsFilter.Gio:
                    startDate = fromDate.Date;
                    endDate = startDate.AddDays(1);
                    break;

                case StatisticsFilter.Ngay:
                    startDate = fromDate.Date.AddDays(-6);
                    endDate = fromDate.Date.AddDays(1);
                    break;

                case StatisticsFilter.Tuan:
                    int diff = (7 + (fromDate.DayOfWeek - DayOfWeek.Monday)) % 7;

                    startDate = fromDate.Date.AddDays(-diff);
                    endDate = startDate.AddDays(7);
                    break;

                case StatisticsFilter.Thang:
                    startDate = new DateTime(fromDate.Year, fromDate.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;

                default:
                    throw new ArgumentException("Bộ lọc thời gian không hợp lệ");
            }

            // 2. Chuyển thời gian Việt Nam sang UTC
            DateTimeOffset startUtc = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified),
                    _timeZone
                ),
                TimeSpan.Zero
            );

            DateTimeOffset endUtc = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified),
                    _timeZone
                ),
                TimeSpan.Zero
            );

            // 3. Thống kê số STT theo dịch vụ
            var result = await _context.Tickets
                .AsNoTracking()
                .Where(s =>
                    //s.UnitId == unitId &&
                    s.IssuanceTime >= startUtc &&
                    s.IssuanceTime < endUtc)
                .GroupBy(s => new
                {
                    s.ServiceId,
                    s.Service!.ServiceName
                })
                .Select(g => new ServiceBarPointDto
                {
                    serviceName = g.Key.ServiceName,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return result;
        }

        private static decimal CalculatePercent(
            int current,
            int previous)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : 100;
            }

            return Math.Round(
                (decimal)(current - previous) / previous * 100,
                2
            );
        }
        public async Task<TrendPointDto> GetTicketTrendAsync()
        {
            var now = DateTimeOffset.UtcNow;

            // Kỳ hiện tại: 3 tháng gần nhất
            var currentFrom = now.AddMonths(-3);
            var currentTo = now;

            // Kỳ trước: 3 tháng liền trước
            var previousFrom = now.AddMonths(-6);
            var previousTo = currentFrom;

            // Query dữ liệu kỳ hiện tại
            var currentQuery = _context.Tickets
                .Where(s =>
                    //s.UnitId == unitId &&
                    s.IssuanceTime >= currentFrom &&
                    s.IssuanceTime < currentTo
                );

            // Query dữ liệu kỳ trước
            var previousQuery = _context.Tickets
                .Where(s =>
                    //s.UnitId == unitId &&
                    s.IssuanceTime >= previousFrom &&
                    s.IssuanceTime < previousTo
                );

            // Kỳ hiện tại
            var currentIssued = await currentQuery
                .CountAsync(s => s.Status == "Completed");

            var currentWaiting = await currentQuery
                .CountAsync(s => s.Status == "Waiting");

            var currentUsed = await currentQuery
                .CountAsync(s => s.Status == "Used");

            var currentSkipped = await currentQuery
                .CountAsync(s => s.Status == "Skipped");

            // Kỳ trước
            var previousIssued = await previousQuery
                .CountAsync(s => s.Status == "Completed");

            var previousWaiting = await previousQuery
                .CountAsync(s => s.Status == "Waiting");

            var previousUsed = await previousQuery
                .CountAsync(s => s.Status == "Used");

            var previousSkipped = await previousQuery
                .CountAsync(s => s.Status == "Skipped");

            // Tính tỷ lệ tăng giảm
            return new TrendPointDto
            {
                percentIssued = CalculatePercent(
                    currentIssued, previousIssued),

                percentWaiting = CalculatePercent(
                    currentWaiting, previousWaiting),

                percentUsed = CalculatePercent(
                    currentUsed, previousUsed),

                percentSkipped = CalculatePercent(
                    currentSkipped, previousSkipped)
            };
        }

        public async Task<DashboardServiceStatisticsDto> GetServiceStatisticsAsync()
        {
            // 1. Điểm đánh giá trung bình theo dịch vụ
            var ratingBars = await _context.Evaluations
                .AsNoTracking()
                //.Where(e => e.unitId == unitId)
                .GroupBy(e => new
                {
                    e.ServiceId,
                    e.Service.ServiceName
                })
                .Select(g => new RatingBarDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    AverageRating = Math.Round(
                        g.Average(e =>
                            ((decimal)e.TimeMark +
                             e.AttitudeMark +
                             e.MaterialMark +
                             e.SatisfedMark) / 4m
                        ),
                        2
                    )
                })
                .OrderByDescending(x => x.AverageRating)
                .ToListAsync();


            // 2. Số phiếu đã sử dụng theo dịch vụ
            var serviceRatios = await _context.Tickets
                .AsNoTracking()
                .Where(s =>
                    //s.unitId == unitId &&
                    s.Status == "Used"
                )
                .GroupBy(s => new
                {
                    s.ServiceId,
                    s.Service.ServiceName
                })
                .Select(g => new ServiceRatioDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    TotalUsed = g.Count()
                })
                .OrderByDescending(x => x.TotalUsed)
                .ToListAsync();


            return new DashboardServiceStatisticsDto
            {
                RatingBars = ratingBars,
                ServiceRatios = serviceRatios
            };
        }
    }
}
