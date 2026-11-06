using Microsoft.EntityFrameworkCore;
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
        public DashboardSvc(MyDbContext context)
        {
            _context = context;
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

        private IQueryable<Sticker> StickerQuery(int unitId, DashboardFilterRequest f)
        {
            var (from, to) = ResolveRange(f);
            var q = _context.stickers.AsNoTracking()
                .Where(s => s.unitId == unitId && s.issuanceTime >= from && s.issuanceTime <= to);

            if (f.ServiceIds is { Count: > 0 })
                q = q.Where(s => f.ServiceIds.Contains(s.serviceId));
            if (f.CounterIds is { Count: > 0 })
                q = q.Where(s => s.counterId != null && f.CounterIds.Contains(s.counterId));
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
        public async Task<TicketSummaryDto> GetTicketSummaryAsync(
    int unitId, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var l = now.ToOffset(Vn);

            var dayStart = new DateTimeOffset(
                l.Year, l.Month, l.Day, 0, 0, 0, Vn).ToUniversalTime();

            var monthStart = new DateTimeOffset(
                l.Year, l.Month, 1, 0, 0, 0, Vn).ToUniversalTime();

            var yearStart = new DateTimeOffset(
                l.Year, 1, 1, 0, 0, 0, Vn).ToUniversalTime();

            var q = _context.stickers.AsNoTracking()
                .Where(s => s.unitId == unitId);

            return new TicketSummaryDto
            {
                TotalIssued = await q.CountAsync(ct),
                IssuedToday = await q.CountAsync(s => s.issuanceTime >= dayStart, ct),
                IssuedThisMonth = await q.CountAsync(s => s.issuanceTime >= monthStart, ct),
                IssuedThisYear = await q.CountAsync(s => s.issuanceTime >= yearStart, ct),
            };
        }

        // ----------------------------------------------------------------
        // DSH-02
        // ----------------------------------------------------------------
        public async Task<TicketStatusDonutDto> GetTicketStatusAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .GroupBy(s => s.status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Recall = g.Sum(x => x.recallCount) })
                .ToListAsync(ct);

            return new TicketStatusDonutDto
            {
                Completed = rows.Where(r => r.Status == StickerStatus.Completed).Sum(r => r.Count),
                Waiting = rows.Where(r => r.Status == StickerStatus.Waiting).Sum(r => r.Count),
                Absent = rows.Where(r => r.Status == StickerStatus.Absent).Sum(r => r.Count),
                Recalled = rows.Sum(r => r.Recall),
            };
        }

        // ----------------------------------------------------------------
        // DSH-03
        // ----------------------------------------------------------------
        public async Task<ChartSeriesDto> GetTicketTrendAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var times = await StickerQuery(unitId, f).Select(s => s.issuanceTime).ToListAsync(ct);

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
        public async Task<CounterServiceSummaryDto> GetCounterServiceSummaryAsync(
            int unitId, CancellationToken ct = default)
        {
            var counters = _context.counters.AsNoTracking().Where(c => c.unitId == unitId && c.isActive);

            return new CounterServiceSummaryDto
            {
                TotalServices = await _context.services.AsNoTracking()
                    .CountAsync(s => s.unitId == unitId && s.isActive, ct),
                TotalCounters = await counters.CountAsync(ct),
                ServingCounters = await counters.CountAsync(c => c.status == CounterStatus.Serving, ct),
                ClosedCounters = await counters.CountAsync(c => c.status == CounterStatus.Closed, ct),
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

            var q = _context.evaluations.AsNoTracking()
                .Where(e => e.unitId == unitId && e.evaluationTime >= from && e.evaluationTime <= to);
            if (f.ServiceIds is { Count: > 0 })
                q = q.Where(e => f.ServiceIds.Contains(e.serviceId));
            if (f.CounterIds is { Count: > 0 })
                q = q.Where(e => f.CounterIds.Contains(e.counterId));

            var evals = await q.Select(e => new
            {
                e.serviceId,
                e.timeMark,
                e.attitudeMark,
                e.materialMark,
                e.satisfedMark
            }).ToListAsync(ct);

            var names = await _context.services.AsNoTracking()
                .Where(s => s.unitId == unitId)
                .ToDictionaryAsync(s => s.id, s => s.serviceName, ct);

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
                .GroupBy(e => e.serviceId)
                .Select(g =>
                {
                    var overalls = g.Select(e => OverallOf(e.timeMark, e.attitudeMark, e.materialMark, e.satisfedMark)).ToList();
                    return new ServiceRatingItemDto
                    {
                        ServiceId = g.Key,
                        ServiceName = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        TotalEvaluations = g.Count(),
                        AvgTime = Avg(g.Select(e => e.timeMark)),
                        AvgAttitude = Avg(g.Select(e => e.attitudeMark)),
                        AvgMaterial = Avg(g.Select(e => e.materialMark)),
                        AvgSatisfied = Avg(g.Select(e => e.satisfedMark)),
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
        // DSH-06
        // ----------------------------------------------------------------
        public async Task<ChartSeriesDto> GetTicketsByServiceAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .Select(s => new { s.serviceId, s.issuanceTime })
                .ToListAsync(ct);

            var names = await _context.services.AsNoTracking()
                .Where(s => s.unitId == unitId)
                .ToDictionaryAsync(s => s.id, s => s.serviceName, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.serviceId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(g.Select(x => (x.issuanceTime, 1d)), f.Granularity, false)
                    }).ToList()
            };
        }

        public async Task<ChartSeriesDto> GetTicketsByCounterAsync(
            int unitId, DashboardFilterRequest f, CancellationToken ct = default)
        {
            var rows = await StickerQuery(unitId, f)
                .Where(s => s.counterId != null)
                .Select(s => new { counterId = s.counterId!, s.issuanceTime })
                .ToListAsync(ct);

            var names = await _context.counters.AsNoTracking()
                .Where(c => c.unitId == unitId)
                .ToDictionaryAsync(c => c.id, c => c.counterName ?? c.counterId, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.counterId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(g.Select(x => (x.issuanceTime, 1d)), f.Granularity, false)
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
                .Where(s => s.callTime != null && s.callTime >= s.issuanceTime)
                .Select(s => new { s.serviceId, s.issuanceTime, call = s.callTime!.Value })
                .ToListAsync(ct);

            var names = await _context.services.AsNoTracking()
                .Where(s => s.unitId == unitId)
                .ToDictionaryAsync(s => s.id, s => s.serviceName, ct);

            return new ChartSeriesDto
            {
                Series = rows.GroupBy(r => r.serviceId)
                    .Select(g => new SeriesDto
                    {
                        Id = g.Key,
                        Name = names.GetValueOrDefault(g.Key, $"#{g.Key}"),
                        Points = ToPoints(
                            g.Select(x => (x.issuanceTime, (x.call - x.issuanceTime).TotalMinutes)),
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
                .Where(s => s.status == StickerStatus.Completed
                         && s.counterId != null
                         && s.callTime != null && s.useTime != null
                         && s.useTime >= s.callTime)
                .Select(s => new
                {
                    counterId = s.counterId!,
                    call = s.callTime!.Value,
                    use = s.useTime!
                })
                .ToListAsync(ct);

            var names = await _context.counters.AsNoTracking()
                .Where(c => c.unitId == unitId)
                .ToDictionaryAsync(c => c.id, c => c.counterName ?? c.counterId, ct);

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

            var q = _context.stickers.AsNoTracking()
                .Where(s => s.unitId == unitId && s.issuanceTime >= from && s.issuanceTime < to);
            if (r.ServiceIds is { Count: > 0 })
                q = q.Where(s => r.ServiceIds.Contains(s.serviceId));

            var times = await q.Select(s => s.issuanceTime).ToListAsync(ct);

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
            var devices = await _context.devices.AsNoTracking()
                .Where(d => d.unitId == unitId)
                .Select(d => new { d.deviceType, d.statusActive, d.statusConnect })
                .ToListAsync(ct);

            return new DeviceSummaryDto
            {
                Total = devices.Count,
                ByType = devices.GroupBy(d => d.deviceType)
                    .Select(g => new DeviceTypeCountDto { DeviceType = g.Key, Count = g.Count() })
                    .ToList(),
                Active = devices.Count(d => d.statusActive == DeviceStatus.Available),
                Inactive = devices.Count(d => d.statusActive == DeviceStatus.Unavailable),
                Connected = devices.Count(d => d.statusConnect == DeviceStatus.Connected),
                Disconnected = devices.Count(d => d.statusConnect == DeviceStatus.Disconnected),
            };
        }

        // DSH-11: popup cảnh báo khi chuyển thiết bị sang "Ngưng hoạt động"
        // mà quầy gắn với thiết bị còn STT chờ/đang xử lý. Trả null nếu không tìm thấy.
        public async Task<DeviceDeactivateCheckDto?> CheckDeviceDeactivateAsync(
            int unitId, int deviceId, CancellationToken ct = default)
        {
            var device = await _context.devices.AsNoTracking()
                .Include(d => d.Counters)
                .FirstOrDefaultAsync(d => d.id == deviceId && d.unitId == unitId, ct);
            if (device == null) return null;

            var counterIds = device.Counters.Select(c => c.id).ToList();
            var today = StartOfDay(DateTimeOffset.UtcNow).ToUniversalTime();

            var pending = await _context.stickers.AsNoTracking()
                .Where(s => s.unitId == unitId
                         && s.counterId != null && counterIds.Contains(s.counterId)
                         && s.issuanceTime >= today
                         && (s.status == StickerStatus.Waiting || s.status == StickerStatus.Processing))
                .GroupBy(s => s.counterId)
                .Select(g => new { CounterId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var names = device.Counters
                .Where(c => pending.Any(p => p.CounterId == c.id))
                .Select(c => c.counterName ?? c.counterId)
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
