using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Profile
{
    public class ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string NewEmail { get; set; } = string.Empty;
    }
}