using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Password
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
