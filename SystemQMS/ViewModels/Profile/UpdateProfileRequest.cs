
using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Profile
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string username { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? email { get; set; }

        [StringLength(10)]
        public string? numberPhone { get; set; }

        public byte[]? avatar { get; set; }
    }
}
