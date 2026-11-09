using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.CrmDto
{
    public class CreateGuestRequestDto
    {
        [Required(ErrorMessage = "Mã đơn vị (unitId) là bắt buộc.")]
        public int unitId { get; set; }

        public string? guestId { get; set; }

        [Required(ErrorMessage = "Họ tên khách hàng không được để trống.")]
        [MaxLength(100)]
        public string guestName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string? phoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [MaxLength(100)]
        public string? email { get; set; }

        [Required(ErrorMessage = "Số CCCD/Định danh là bắt buộc.")]
        [MaxLength(100)]
        public string CCCD { get; set; } = string.Empty;

        public string? password { get; set; }

        public string? status { get; set; } = "Active";
    }
}