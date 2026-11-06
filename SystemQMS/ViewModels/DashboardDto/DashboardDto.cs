namespace SystemQMS.ViewModels.DashboardDto
{
    public class DashboardDto
    {
        // ===== Request =====
        public enum DashboardGranularity { Hour, Day, Month, Year }
        public enum HeatmapRange { Day, Week }

        public class DashboardFilterRequest          // dùng chung DSH-01/03/05/06/07/08
        {
            public DateTimeOffset? From { get; set; }
            public DateTimeOffset? To { get; set; }
            public DashboardGranularity Granularity { get; set; } = DashboardGranularity.Day;
            public List<int>? ServiceIds { get; set; }
            public List<int>? CounterIds { get; set; }
            // unitId lấy từ token, không nhận từ client
        }

        public class HeatmapRequest                  // DSH-09
        {
            public HeatmapRange Range { get; set; }
            public DateTimeOffset? Date { get; set; }
            public List<int>? ServiceIds { get; set; }
        }

        // ===== Dùng chung =====
        public class TimeSeriesPointDto { public DateTimeOffset Time { get; set; } public double Value { get; set; } }
        public class SeriesDto { public int? Id { get; set; } public string Name { get; set; } public List<TimeSeriesPointDto> Points { get; set; } }
        public class ChartSeriesDto { public List<SeriesDto> Series { get; set; } }   // DSH-03/06/07/08 (đường/cột)

        // ===== STT =====
        public class TicketSummaryDto           // DSH-01
        {
            public int TotalIssued { get; set; }
            public int IssuedToday { get; set; }
            public int IssuedThisMonth { get; set; }
            public int IssuedThisYear { get; set; }
        }
        public class TicketStatusDonutDto       // DSH-02
        {
            public int Completed { get; set; }
            public int Waiting { get; set; }
            public int Absent { get; set; }
            public int Recalled { get; set; }   // SUM(recallCount)
        }

        // ===== Quầy / dịch vụ =====
        public class CounterServiceSummaryDto   // DSH-04
        {
            public int TotalServices { get; set; }
            public int TotalCounters { get; set; }
            public int ServingCounters { get; set; }
            public int ClosedCounters { get; set; }
        }
        public class ServiceRatingItemDto       // DSH-05
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public int TotalEvaluations { get; set; }
            public double AvgTime { get; set; }
            public double AvgAttitude { get; set; }
            public double AvgMaterial { get; set; }
            public double AvgSatisfied { get; set; }
            public double AvgOverall { get; set; }
            public double HighRatingRate { get; set; }   // % đánh giá >= ngưỡng "tốt"
        }
        public class ServiceRatingDashboardDto
        {
            public int HighRatedServiceCount { get; set; }
            public List<ServiceRatingItemDto> Items { get; set; }
        }
        // DSH-07: ChartSeriesDto (mỗi series = 1 dịch vụ, value = phút chờ TB)
        // DSH-08: ChartSeriesDto (mỗi series = 1 quầy, value = phút xử lý TB)

        public class HeatmapCellDto { public int DayOfWeek { get; set; } public int Hour { get; set; } public int Count { get; set; } public double Rate { get; set; } }
        public class HeatmapDto     // DSH-09
        {
            public List<HeatmapCellDto> Cells { get; set; }
            public int MaxCount { get; set; }
        }

        // ===== Thiết bị =====
        public class DeviceTypeCountDto { public string DeviceType { get; set; } public int Count { get; set; } }
        public class DeviceSummaryDto           // DSH-10/11/12
        {
            public int Total { get; set; }
            public List<DeviceTypeCountDto> ByType { get; set; }   // DSH-10
            public int Active { get; set; }                        // DSH-11
            public int Inactive { get; set; }
            public int Connected { get; set; }                     // DSH-12
            public int Disconnected { get; set; }
        }
        public class DeviceDeactivateCheckDto   // DSH-11: popup cảnh báo khi chuyển sang Ngưng hoạt động
        {
            public bool HasPendingTickets { get; set; }
            public int PendingTicketCount { get; set; }
            public List<string> CounterNames { get; set; }
        }
    }
}
