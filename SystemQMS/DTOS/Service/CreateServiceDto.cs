using System.ComponentModel.DataAnnotations;

namespace SystemQMS.DTOs.Service
{
    public class CreateServiceDto
    {
        public int unitId { get; set; } = 1;

        [Required(ErrorMessage = "Mã dịch vụ không được để trống")]
        [MaxLength(100, ErrorMessage = "Mã dịch vụ tối đa 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9\s_-]+$", ErrorMessage = "Mã dịch vụ chỉ gồm chữ, số, dấu '-', '_' và khoảng trắng")]
        public string serviceId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên dịch vụ tối đa 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Tên dịch vụ chỉ gồm chữ, số, dấu '-' và '_'")]
        public string serviceName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? description { get; set; }

        public byte[]? image { get; set; }

        public bool isActive { get; set; } = true;

        [Required(ErrorMessage = "Chế độ chia quầy không được để trống")]
        public string sharingLink { get; set; } = "Chỉ định quầy khi cấp số"; // Hoặc "Phân quầy khi gọi số"

        public DateTimeOffset averageTime { get; set; } = DateTimeOffset.UtcNow;
    }
}