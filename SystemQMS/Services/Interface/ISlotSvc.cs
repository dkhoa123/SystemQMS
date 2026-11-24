using SystemQMS.ViewModels.IntergrationsDto;

namespace SystemQMS.Services.Interface
{
    public interface ISlotSvc
    {
        Task<ReserveSlotResponse> ReserveSlotAsync(int unitId, ReserveSlotRequest request);
    }
}
