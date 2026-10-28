using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Services.Interface
{
    public interface IServiceSvc
    {
        Task<IEnumerable<ServiceResponseDto>> GetAllServicesAsync(int? unitId, int? counterId, bool? isActive, string? search);
        
        Task<ServiceDetailDto?> GetServiceByIdAsync(int id, string? ticketStatus = null, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null);
        
        Task<(bool Success, string Message, int? NewId, string? ServiceId)> CreateServiceAsync(CreateServiceDto dto);
        
        Task<(bool Success, string Message)> UpdateServiceAsync(int id, UpdateServiceDto dto);
        
        Task<(bool Success, string Message)> DeleteServiceAsync(int id);
    }
}