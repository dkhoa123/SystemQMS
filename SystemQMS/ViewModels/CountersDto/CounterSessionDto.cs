using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.CountersDto
{
    public class OpenSessionRequestDto
    {
        [Required(ErrorMessage = "Mã cán bộ (userId) không được để trống")]
        public int userId { get; set; }
    }

    public class CounterSessionResponseDto
    {
        public int counterId { get; set; }
        public string counterCode { get; set; } = string.Empty;
        public string counterName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public bool isActive { get; set; }
        public int? currentUserId { get; set; }
        public string? currentUserName { get; set; }
        public DateTimeOffset sessionStartTime { get; set; }
    }
}