using SystemQMS.ViewModels.CrmDto;

namespace SystemQMS.Services.Interface
{
    public interface IGuestSvc
    {
        Task<PagedResultDto<GuestDto>> GetGuestsAsync(GuestQueryFilterDto filter);
        Task<GuestDto?> GetGuestByIdAsync(int id);
        Task<GuestDto?> GetGuestByCccdAsync(string cccd);
        Task<GuestDto> CreateGuestAsync(CreateGuestRequestDto request);
        Task<GuestDto> UpdateGuestAsync(int id, UpdateGuestRequestDto request);
        Task DeleteGuestAsync(int id);
    }
}