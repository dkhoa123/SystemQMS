using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.NumberFomular
{
    public class CreateNumberFomularDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumberType { get; set; } = "Prefix";

        [MaxLength(3)]
        public string? FirstName { get; set; }

        [MaxLength(3)]
        public string? LastName { get; set; }

        public int StartNumber { get; set; }

        public int EndNumber { get; set; }

        public bool IsDailyLimitEnabled { get; set; } = false;

        public int? DailyEndNumber { get; set; }

        public DateTimeOffset? LimitDay { get; set; }

        [Required]
        [RegularExpression("^(Shared|Independent)$")]
        public string SequenceMode { get; set; } = "Independent";

        // ServiceCode hoặc Custom
        [Required]
        [RegularExpression("^(ServiceCode|Custom)$")]
        public string FormatSource { get; set; } = "ServiceCode";
    }
}