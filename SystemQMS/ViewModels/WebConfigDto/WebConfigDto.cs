using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.WebConfigDto
{
    public class WebConfigDto
    {
        // --- WEB-01: Thông tin đơn vị---
        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        public string UnitName { get; set; }

        [Required(ErrorMessage = "Tên hiển thị là bắt buộc")]
        public string ShowName { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Số điện thoại hỗ trợ là bắt buộc")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
        public string PhoneHelp { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string EmailHelp { get; set; }

        // --- WEB-02: Giao diện Web App---
        public string Favicon { get; set; }

        [MaxLength(255)]
        public string Slogan { get; set; }

        [Url(ErrorMessage = "Website phải đúng định dạng URL")]
        public string Website { get; set; }

        [Required(ErrorMessage = "Tiêu đề trang chủ là bắt buộc")]
        public string TitleHome { get; set; }

        public string DescriptionHome { get; set; }
        public string BackgroundImage { get; set; }
        public string BannerText { get; set; }

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Màu chủ đạo phải là mã HEX hợp lệ")]
        public string ToneColorHex { get; set; }

        // --- WEB-05, WEB-06, WEB-08: Phương thức bốc số online ---
        public string BookingMethod { get; set; } // "Theo khung giờ" / "Không theo khung giờ"
        public int? SlotDuration { get; set; }
        public int? MaxGuestsPerSlot { get; set; }
        public int? AdvanceBookingDays { get; set; }
        public bool AllowImmediateBooking { get; set; }
        public int? MaxBookingsPerUserDaily { get; set; }
        public string AppliedScope { get; set; } // "Tất cả" / "Tùy chọn"
    }
}