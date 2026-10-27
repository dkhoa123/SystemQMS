using SystemQMS.Dtos;

namespace SystemQMS.Services.Interface
{
    public interface ICounterSvc
    {
        // CNT-01
        Task<List<CounterResponse>> GetCountersAsync(
            int unitId, string? status = null, bool? isActive = null, string? search = null);

        // CNT-02
        Task<CounterResponse> CreateCounterAsync(CreateCounterRequest request);

        // CNT-03
        Task<CounterResponse> UpdateCounterAsync(int id, UpdateCounterRequest request);

        // CNT-04a
        Task<CounterDetailResponse> GetCounterAsync(int id);

        // CNT-04b
        Task<List<CounterStickerResponse>> GetCounterStickersAsync(
            int id, string? status = null, DateTimeOffset? from = null, DateTimeOffset? to = null);

        // CNT-05
        Task DeleteCounterAsync(int id, CancellationToken ct = default);
    }
}
