using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.NumberFomular
{
    public class UpdateNumberFomularDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumberType { get; set; } = "Prefix";

        [MaxLength(3)]
        public string? FirstName { get; set; }

        [MaxLength(3)]
        public string? LastName { get; set; }

        public int StartNumber { get; set; }

        public int EndNumber { get; set; }

        public bool IsDailyLimitEnabled { get; set; }

        public int? DailyEndNumber { get; set; }

        public DateTimeOffset? LimitDay { get; set; }

        [Required]
        [RegularExpression("^(Shared|Independent)$")]
        public string SequenceMode { get; set; } = "Independent";

        [Required]
        [RegularExpression("^(ServiceCode|Custom)$")]
        public string FormatSource { get; set; } = "ServiceCode";
    }
}