using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.CrmDto
{
    public class UpdateGuestRequestDto
    {
        [Required(ErrorMessage = "Họ tên khách hàng không được để trống.")]
        [MaxLength(100)]
        public string guestName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? phoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(100)]
        public string? email { get; set; }

        [Required(ErrorMessage = "Số CCCD/Định danh là bắt buộc.")]
        [MaxLength(100)]
        public string CCCD { get; set; } = string.Empty;

        public string? status { get; set; }
    }
}