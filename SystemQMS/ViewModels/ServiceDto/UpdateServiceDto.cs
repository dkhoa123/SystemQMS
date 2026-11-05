using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.ServiceDto
{
    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên dịch vụ tối đa 100 ký tự")]
        public string serviceName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? description { get; set; }
        public byte[]? image { get; set; }
        public bool isActive { get; set; }
        [Required(ErrorMessage = "Chế độ chia quầy không được để trống")]
        public string sharingLink { get; set; } = "Chỉ định quầy khi cấp số";
        public DateTimeOffset averageTime { get; set; } = DateTimeOffset.UtcNow;
    }
}