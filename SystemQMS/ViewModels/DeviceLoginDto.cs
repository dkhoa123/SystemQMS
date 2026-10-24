using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels
{
    public class DeviceLoginDto
    {
        [Required]
        public string deviceId { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "Mã đăng nhập phải gồm 6 chữ số")]
        public string code { get; set; } = string.Empty;
    }
}
