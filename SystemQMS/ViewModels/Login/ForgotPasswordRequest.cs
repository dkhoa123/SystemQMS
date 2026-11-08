using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Login
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
