using SystemQMS.ViewModels.CounterSessionDto;

namespace SystemQMS.Services.Interface
{
    public interface ICounterSessionSvc
    {
        Task<string> TakeControl(CounterSessionDto model);

        Task<string> ReleaseControl(int counterId, int userId);
    }
}