using SystemQMS.ViewModels.PageResultDto;
using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Services.Interface
{
    public interface IServiceSvc
    {
        public Task<PageResultDto<ServiceResponseDto>> GetAllServicesAsync(FilterServiceDto filter);
        
        Task<ServiceDetailDto?> GetServiceByIdAsync(int id);

        Task<PageResultDto<ServiceHistoryDto>> GetServiceByIdAsync(int id, FilterServiceDto filter);
        
        //Task<(bool Success, string Message, int? NewId, string? ServiceId)> CreateServiceAsync(CreateServiceDto dto);
        
        Task<ServiceResponseDto> UpdateServiceAsync(int id, UpdateServiceDto dto);
        
        Task<(bool Success, string Message)> DeleteServiceAsync(int id);
    }
}