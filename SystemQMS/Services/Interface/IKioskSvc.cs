using SystemQMS.Models;
using SystemQMS.ViewModels.KioskDto;

namespace SystemQMS.Services.Interface
{
    public interface IKioskSvc
    {
        public List<ServiceKioskDto> GetServiceKiosks(string kioskDeviceId);
        public Task<TicketReceiptDto> CreateTicketAsync(int serviceId, int counterId, int userId);
        public bool ReportDeviceStatus(string kioskDeviceId, string status);
    }
}
