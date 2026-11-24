using SystemQMS.ViewModels.CountersDto;
using SystemQMS.ViewModels.DspDto;
using SystemQMS.ViewModels.PageResultDto;

namespace SystemQMS.Services.Interface
{
    public interface ICounterSvc
    {
        // CNT-01
        public PageResultDto<CounterResponse> GetCountersAsync(FilterCounterDto filter);

        // CNT-05
        Task DeleteCounterAsync(int id, CancellationToken ct = default);
    }
}
