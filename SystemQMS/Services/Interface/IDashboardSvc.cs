using static SystemQMS.ViewModels.DashboardDto.DashboardDto;

namespace SystemQMS.Services.Interface
{
    public interface IDashboardSvc
    {
        public  Task<TicketSummaryDto> GetTicketSummaryAsync(int unitId, CancellationToken ct = default);                       // DSH-01
        public Task<TicketStatusDonutDto> GetTicketStatusAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default); // DSH-02
        public Task<ChartSeriesDto> GetTicketTrendAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default);        // DSH-03
        public Task<CounterServiceSummaryDto> GetCounterServiceSummaryAsync(int unitId, CancellationToken ct = default);       // DSH-04
        public Task<ServiceRatingDashboardDto> GetServiceRatingsAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default); // DSH-05
        public Task<ChartSeriesDto> GetTicketsByServiceAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default);   // DSH-06
        public Task<ChartSeriesDto> GetTicketsByCounterAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default);   // DSH-06
        public Task<ChartSeriesDto> GetWaitTimeByServiceAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default);  // DSH-07
        public Task<ChartSeriesDto> GetProcessTimeByCounterAsync(int unitId, DashboardFilterRequest f, CancellationToken ct = default); // DSH-08
        public Task<HeatmapDto> GetPeakHoursAsync(int unitId, HeatmapRequest r, CancellationToken ct = default);               // DSH-09
        public Task<DeviceSummaryDto> GetDeviceSummaryAsync(int unitId, CancellationToken ct = default);                        // DSH-10/11/12
        public Task<DeviceDeactivateCheckDto?> CheckDeviceDeactivateAsync(int unitId, int deviceId, CancellationToken ct = default);  // DSH-11 popup

    }
}
