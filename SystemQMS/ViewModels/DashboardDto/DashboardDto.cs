namespace SystemQMS.ViewModels.DashboardDto
{
    public class DashboardDto
    {
        // ===== Request =====
        public enum DashboardGranularity { Hour, Day, Month, Year }
        public enum HeatmapRange { Day, Week }

        // ===== STT =====
        public enum StatisticsFilter {
            Gio, Ngay, Tuan, Thang
        }

        public class DashboardFilterRequest   
        {
            public DateTimeOffset? From { get; set; }
            public DateTimeOffset? To { get; set; }
            public DashboardGranularity Granularity { get; set; } = DashboardGranularity.Day;
            public List<int>? ServiceIds { get; set; }
            public List<int>? CounterIds { get; set; }
        }
        public class TicketSummaryDto           // DSH-01
        {
            public string time { get; set; }   // "2023-01-01 00:00:00" (theo múi giờ của client)
            public int value { get; set; }     // số lượng STT đã phát trong khoảng thời gian
        }
        public class TicketStatusDonutDto       // DSH-02
        {
            public int totalIssued { get; set; }
            public int totalWaiting { get; set; }
            public int totalUsed { get; set; }
            public int totalSkipped { get; set; }
        }

        // ===== Quầy / dịch vụ =====
        public class CounterServiceSummaryDto   // DSH-04
        {
            public int totalDevices { get; set; }
            public int totalServices { get; set; }
            public int totalTickets { get; set; }
            public int statusDeviceActive { get; set; }
            public int statusDeviceInactive { get; set; }
            public int statusServiceActive { get; set; }
            public int statusServiceInactive { get; set; }
            public int statusTicketIssued { get; set; }
            public int statusTicketUsed { get; set; }
            public int statusTicketSkipped { get; set; }
        }

        public class ServiceBarPointDto {
            public string serviceName { get; set; }
            public int count { get; set; }
        }

        public class TrendPointDto
        {
            public decimal percentIssued { get; set; }
            public decimal percentWaiting { get; set; }
            public decimal percentUsed { get; set; }
            public decimal percentSkipped { get; set; }
            
        }

        public class DashboardServiceStatisticsDto
        {

            public List<RatingBarDto> RatingBars { get; set; } = new();

            public List<ServiceRatioDto> ServiceRatios { get; set; } = new();
        }

        public class RatingBarDto
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public decimal AverageRating { get; set; }
        }

        public class ServiceRatioDto
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public int TotalUsed { get; set; }
        }
    }
}
