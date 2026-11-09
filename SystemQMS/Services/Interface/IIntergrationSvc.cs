using SystemQMS.ViewModels.IntergrationsDto;

namespace SystemQMS.Services.Interfaces
{
    public interface IIntegrationSvc
    {
        Task<List<CustomerDto>> GetCrmCustomersAsync(int unitId, string keyword);
        Task<List<ServiceScheduleDto>> GetQmsServicesAsync(int unitId);
    }
}