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

        private static (DateTimeOffset from, DateTimeOffset to) ResolveRange(DashboardFilterRequest f)
        {
            var now = DateTimeOffset.UtcNow;

            var from = (f.From ?? StartOfDay(now)).ToUniversalTime();
            var to = (f.To ?? now).ToUniversalTime();

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

        private IQueryable<Tickets> StickerQuery(int unitId, DashboardFilterRequest f)
        {
            var (from, to) = ResolveRange(f);
            var q = _context.Tickets.AsNoTracking()
                .Where(s => s.UnitId == unitId && s.IssuanceTime >= from && s.IssuanceTime <= to);

            if (f.ServiceIds is { Count: > 0 })
                q = q.Where(s => f.ServiceIds.Contains(s.ServiceId));
            if (f.CounterIds is { Count: > 0 })
                q = q.Where(s => s.CounterId != null && f.CounterIds.Contains(s.CounterId));
            return q;
        }

        private static List<TimeSeriesPointDto> ToPoints(
            IEnumerable<(DateTimeOffset time, double value)> rows,
            DashboardGranularity g, bool average)
        {
            return rows
                .GroupBy(r => Bucket(r.time, g))
                .OrderBy(x => x.Key)
                .Select(x => new TimeSeriesPointDto
                {
                    Time = x.Key,
                    Value = average ? Math.Round(x.Average(v => v.value), 2) : x.Count()
                })
                .ToList();
        }

        // ----------------------------------------------------------------
        // DSH-01
        // ----------------------------------------------------------------
        public async Task<List<TicketSummaryDto>> GetTicketIssuedAsync(int unitId, StatisticsFilter filter, DateOnly fromDate, DateOnly toDate)
        {
            if (fromDate > toDate)
            {
                throw new ArgumentException(
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc."
                );
            }

            // 1. Chuyển khoảng ngày Việt Nam sang UTC
            DateTime localStart = fromDate.ToDateTime(
                TimeOnly.MinValue
            );

            DateTime localEnd = toDate.AddDays(1).ToDateTime(
                TimeOnly.MinValue
            );

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
            DateOnly fromDate,
            DateOnly toDate)
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
                    $"Tuần {ISOWeek.GetWeekOfYear(time)}/{ISOWeek.GetYear(time)}",

                StatisticsFilter.Thang =>
                    time.ToString("MM/yyyy"),

                _ => time.ToString("dd/MM/yyyy")
            };
        }

        // ----------------------------------------------------------------
        // DSH-02
        // ----------------------------------------------------------------
        public async Task<TicketStatusDonutDto> GetTicketStatusAsync()
        {
            var rows = _context.Tickets.AsQueryable();
                

            return new TicketStatusDonutDto
            {
                totalIssued = await rows.CountAsync(s => s.Stage == "Completed"),
                totalWaiting = await rows.CountAsync(s => s.Stage == "Waiting"),
                totalUsed = await rows.CountAsync(s => s.Stage == "Used"),
                totalSkipped = await rows.CountAsync(s => s.Stage == "Skipped"),

            };
        }

        // ----------------------------------------------------------------
        // DSH-03
        // ----------------------------------------------------------------
        public async Task<ChartSeriesDto> GetTicketTrendAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var times = await StickerQuery(unitId, f).Select(s => s.IssuanceTime).ToListAsync(ct);

            return new ChartSeriesDto
            {
                Series = new List<SeriesDto>
                {
                    new()
                    {
                        Name = "Tổng STT đã cấp",
                        Points = ToPoints(times.Select(t => (t, 1d)), f.Granularity, average: false)
                    }
                }
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
                statusTicketIssued = await _context.Tickets.CountAsync(s => s.Stage == "Completed"),
                statusTicketUsed = await _context.Tickets.CountAsync(s => s.Stage == "Used"),
                statusTicketSkipped = await _context.Tickets.CountAsync(s => s.Stage == "Skipped"),
            };
        }


        // ----------------------------------------------------------------
        // DSH-05
        // Điểm <= 0 coi là "không đánh giá", loại khỏi trung bình.
        // ----------------------------------------------------------------
        public async Task<ServiceRatingDashboardDto> GetServiceRatingsAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var (from, to) = ResolveRange(f);

            var q = _context.Evaluations.AsNoTracking()
                .Where(e => e.UnitId == unitId && e.EvaluationTime >= from && e.EvaluationTime <= to);
            if (f.ServiceIds is { Count: > 0 })
                q = q.Where(e => f.ServiceIds.Contains(e.ServiceId));
            if (f.CounterIds is { Count: > 0 })
                q = q.Where(e => f.CounterIds.Contains(e.CounterId));

            var evals = await q.Select(e => new
            {
                e.ServiceId,
                e.TimeMark,
                e.AttitudeMark,
                e.MaterialMark,
                e.SatisfedMark
            }).ToListAsync(ct);

            var names = await _context.Services.AsNoTracking()
                .Where(s => s.UnitId == unitId)
                .ToDictionaryAsync(s => s.Id, s => s.ServiceName, ct);

            static double Avg(IEnumerable<int> marks)
            {
                var valid = marks.Where(m => m > 0).ToList();
                return valid.Count == 0 ? 0 : Math.Round(valid.Average(), 2);
            }

            static double OverallOf(int t, int a, int m, int s)
            {
                var valid = new[] { t, a, m, s }.Where(x => x > 0).ToList();
                return valid.Count == 0 ? 0 : valid.Average();
            }

            var items = evals
                .GroupBy(e => e.ServiceId)
                .Select(g =>
                {
                    var overalls = g.Select(e => OverallOf(e.TimeMark, e.AttitudeMark, e.MaterialMark, e.SatisfedMark)).ToList();
                    return new ServiceRatingItemDto
                    {
                        ServiceId = g.Key,
                        ServiceName = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        TotalEvaluations = g.Count(),
                        AvgTime = Avg(g.Select(e => e.TimeMark)),
                        AvgAttitude = Avg(g.Select(e => e.AttitudeMark)),
                        AvgMaterial = Avg(g.Select(e => e.MaterialMark)),
                        AvgSatisfied = Avg(g.Select(e => e.SatisfedMark)),
                        AvgOverall = Math.Round(overalls.Where(o => o > 0).DefaultIfEmpty(0).Average(), 2),
                        HighRatingRate = Math.Round(
                            100.0 * overalls.Count(o => o >= HighRatingThreshold) / g.Count(), 2),
                    };
                })
                .OrderByDescending(i => i.AvgOverall)
                .ToList();

            return new ServiceRatingDashboardDto
            {
                HighRatedServiceCount = items.Count(i => i.AvgOverall >= HighRatingThreshold),
                Items = items,
            };
        }

        // ----------------------------------------------------------------
        // DSH-06: Thống kê STT cấp theo dịch vụ
        // ----------------------------------------------------------------
        public async Task<List<ServiceBarPointDto>> GetTicketsByServiceAsync(
            int unitId,
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
                    s.UnitId == unitId &&
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

        public async Task<ChartSeriesDto> GetTicketsByCounterAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .Where(s => s.CounterId != null)
                .Select(s => new { counterId = s.CounterId!, s.IssuanceTime })
                .ToListAsync(ct);

            var names = await _context.Counters.AsNoTracking()
                .Where(c => c.UnitId == unitId)
                .ToDictionaryAsync(c => c.Id, c => c.CounterName ?? c.CounterId, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.counterId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(g.Select(x => (x.IssuanceTime, 1d)), f.Granularity, false)
                    }).ToList()
            };
        }

        // ----------------------------------------------------------------
        // DSH-07: thời gian chờ TB = callTime - issuanceTime (phút), theo dịch vụ
        // ----------------------------------------------------------------
        public async Task<ChartSeriesDto> GetWaitTimeByServiceAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .Where(s => s.CallTime != null && s.CallTime >= s.IssuanceTime)
                .Select(s => new { s.ServiceId, s.IssuanceTime, call = s.CallTime!.Value })
                .ToListAsync(ct);

            var names = await _context.Services.AsNoTracking()
                .Where(s => s.UnitId == unitId)
                .ToDictionaryAsync(s => s.Id, s => s.ServiceName, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.ServiceId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(
                            g.Select(x => (x.IssuanceTime, (x.call - x.IssuanceTime).TotalMinutes)),
                            f.Granularity, average: true)
                    }).ToList()
            };
        }

        // ----------------------------------------------------------------
        // DSH-08: thời gian xử lý TB = useTime - callTime (phút), theo quầy
        // Chỉ tính STT Hoàn thành (Vắng không phản ánh thời gian xử lý thật).
        // ----------------------------------------------------------------
        public async Task<ChartSeriesDto> GetProcessTimeByCounterAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .Where(s => s.Status == StickerStatus.Completed
                         && s.CounterId != null
                         && s.CallTime != null && s.UseTime != null
                         && s.UseTime >= s.CallTime)
                .Select(s => new
                {
                    counterId = s.CounterId!,
                    call = s.CallTime!.Value,
                    use = s.UseTime!
                })
                .ToListAsync(ct);

            var names = await _context.Counters.AsNoTracking()
                .Where(c => c.UnitId == unitId)
                .ToDictionaryAsync(c => c.Id, c => c.CounterName ?? c.CounterId, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.counterId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(
                            g.Select(x => (x.call, (x.use - x.call).TotalMinutes)),
                            f.Granularity, average: true)
                    }).ToList()
            };
        }

        // ----------------------------------------------------------------
        // DSH-09: heatmap. Range=Day: 1 ngày; Range=Week: tuần (T2-CN) chứa Date.
        // DayOfWeek: 0=CN ... 6=T7 (theo System.DayOfWeek). Chỉ trả ô có dữ liệu.
        // ----------------------------------------------------------------
        public async Task<HeatmapDto> GetPeakHoursAsync(
            int unitId, HeatmapRequest r, CancellationToken ct = default)
        {
            var anchor = StartOfDay(r.Date ?? DateTimeOffset.UtcNow);
            DateTimeOffset from, to;

            if (r.Range == HeatmapRange.Week)
            {
                int diff = ((int)anchor.DayOfWeek + 6) % 7;     // lùi về thứ Hai
                from = anchor.AddDays(-diff);
                to = from.AddDays(7);
            }
            else
            {
                from = anchor;
                to = anchor.AddDays(1);
            }

            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            var q = _context.Tickets.AsNoTracking()
                .Where(s => s.UnitId == unitId && s.IssuanceTime >= from && s.IssuanceTime < to);
            if (r.ServiceIds is { Count: > 0 })
                q = q.Where(s => r.ServiceIds.Contains(s.ServiceId));

            var times = await q.Select(s => s.IssuanceTime).ToListAsync(ct);

            var cells = times
                .Select(t => t.ToOffset(Vn))
                .GroupBy(t => new { Dow = (int)t.DayOfWeek, t.Hour })
                .Select(g => new HeatmapCellDto
                {
                    DayOfWeek = g.Key.Dow,
                    Hour = g.Key.Hour,
                    Count = g.Count()
                })
                .ToList();

            int total = cells.Sum(c => c.Count);
            foreach (var c in cells)
                c.Rate = total == 0 ? 0 : Math.Round(100.0 * c.Count / total, 2);

            return new HeatmapDto
            {
                Cells = cells.OrderBy(c => c.DayOfWeek).ThenBy(c => c.Hour).ToList(),
                MaxCount = cells.Count == 0 ? 0 : cells.Max(c => c.Count),
            };
        }

        // ----------------------------------------------------------------
        // DSH-10 / 11 / 12
        // ----------------------------------------------------------------
        public async Task<DeviceSummaryDto> GetDeviceSummaryAsync(int unitId, CancellationToken ct = default)
        {
            var devices = await _context.Devices.AsNoTracking()
                .Where(d => d.UnitId == unitId)
                .Select(d => new { d.DeviceType, d.StatusActive, d.StatusConnect })
                .ToListAsync(ct);

            return new DeviceSummaryDto
            {
                Total = devices.Count,
                ByType = devices.GroupBy(d => d.DeviceType)
                    .Select(g => new DeviceTypeCountDto { DeviceType = g.Key, Count = g.Count() })
                    .ToList(),
                Active = devices.Count(d => d.StatusActive == DeviceStatus.Available),
                Inactive = devices.Count(d => d.StatusActive == DeviceStatus.Unavailable),
                Connected = devices.Count(d => d.StatusConnect == DeviceStatus.Connected),
                Disconnected = devices.Count(d => d.StatusConnect == DeviceStatus.Disconnected),
            };
        }

        // DSH-11: popup cảnh báo khi chuyển thiết bị sang "Ngưng hoạt động"
        // mà quầy gắn với thiết bị còn STT chờ/đang xử lý. Trả null nếu không tìm thấy.
        public async Task<DeviceDeactivateCheckDto?> CheckDeviceDeactivateAsync(
            int unitId, int deviceId, CancellationToken ct = default)
        {
            var device = await _context.Devices.AsNoTracking()
                .Include(d => d.Counters)
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.UnitId == unitId, ct);
            if (device == null) return null;

            var counterIds = device.Counters.Select(c => c.Id).ToList();
            var today = StartOfDay(DateTimeOffset.UtcNow).ToUniversalTime();

            var pending = await _context.Tickets.AsNoTracking()
                .Where(s => s.UnitId == unitId
                         && s.CounterId != null && counterIds.Contains(s.CounterId)
                         && s.IssuanceTime >= today
                         && s.Status == StickerStatus.Waiting)
                .GroupBy(s => s.CounterId)
                .Select(g => new { CounterId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var names = device.Counters
                .Where(c => pending.Any(p => p.CounterId == c.Id))
                .Select(c => c.CounterName ?? c.CounterId)
                .ToList();

            return new DeviceDeactivateCheckDto
            {
                HasPendingTickets = pending.Count > 0,
                PendingTicketCount = pending.Sum(p => p.Count),
                CounterNames = names,
            };
        }

        
    }
}
