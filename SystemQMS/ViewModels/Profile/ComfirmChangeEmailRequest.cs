using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Profile
{
    public class ConfirmChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }
}