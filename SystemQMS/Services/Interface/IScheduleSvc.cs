using SystemQMS.ViewModels.ScheduleDto;
namespace SystemQMS.Services.Interface
{
    public interface IScheduleSvc
    {
        //Thời điểm hiện tại có nằm trong ca hoạt động của dịch vụ không
        // Dịch vụ chưa cấu hình ca nào -> coi như không giới hạn giờ (true) 
        Task<bool> IsWithinOperatingHoursAsync(int serviceId, CancellationToken ct = default);

        // Kiểm tra đầy đủ cho màn hình kiosk: trong giờ + còn quầy mở phục vụ dịch vụ
        Task<ServiceAvailabilityResponse> CheckServiceAvailabilityAsync(int serviceId, CancellationToken ct = default);
    }
}
