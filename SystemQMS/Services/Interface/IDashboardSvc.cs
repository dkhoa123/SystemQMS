using static SystemQMS.ViewModels.DashboardDto.DashboardDto;

namespace SystemQMS.Services.Interface
{
    public interface IDashboardSvc
    {
        public  Task<List<TicketSummaryDto>> GetTicketIssuedAsync(int unitId, StatisticsFilter filter, DateOnly? fromDate, DateOnly? toDate);                       // DSH-01
        public Task<TicketStatusDonutDto> GetTicketStatusAsync(); // DSH-02
        
        public Task<CounterServiceSummaryDto> GetCounterServiceSummaryAsync(); // DSH-04

        Task<List<ServiceBarPointDto>> GetTicketsByServiceAsync(StatisticsFilter filter, DateTime fromDate); // DSH-06

        public Task<TrendPointDto> GetTicketTrendAsync();        // DSH-03

        Task<DashboardServiceStatisticsDto> GetServiceStatisticsAsync();


    }
}
