using SystemQMS.ViewModels.NotificationDto;

namespace SystemQMS.Services.Interfaces
{
    public interface INotificationSvc
    {
        Task<IEnumerable<NotificationResponseDto>> GetAllAsync(int unitId);
        Task<NotificationResponseDto> GetByIdAsync(int id, int unitId);
        Task<NotificationResponseDto> CreateAsync(int unitId, NotificationCreateDto dto);
        Task<bool> UpdateAsync(int id, int unitId, NotificationUpdateDto dto);
        Task<bool> DeleteAsync(int id, int unitId);
        Task<bool> RetryFailedAsync(int id, int unitId);
    }
}