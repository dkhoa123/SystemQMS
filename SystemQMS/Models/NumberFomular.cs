using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class NumberFomular
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        [MaxLength(100)]
        public string NumberType { get; set; }
        [MaxLength(20)]
        public string FirstName { get; set; }
        [MaxLength(20)]
        public string LastName { get; set; }
        public int StartNumber { get; set; }
        public int EndNumber { get; set; }
        public bool IsDailyLimitEnabled { get; set; } = false;

        public int? DailyEndNumber { get; set; }
        public DateTimeOffset LimitDay { get; set; }
        [MaxLength(20)]
        public string SequenceMode { get; set; } = "Independent";

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

    }
}
