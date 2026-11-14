using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Tickets
    {
        [Key]
        [MaxLength(100)]
        public string SttId { get; set; }
        public int UnitId { get; set; }
        public int ServiceId { get; set; }
        public int CounterId { get; set; }
        public int UserId { get; set; }

        public int TicketNumber { get; set; }
        public DateTimeOffset IssuanceTime { get; set; }
        public DateTimeOffset UseTime { get; set; }

        public DateTimeOffset? CallTime { get; set; }

        public int RecallCount { get; set; }
        [MaxLength(100)]
        public string Status { get; set; }
        [MaxLength(100)]
        public string Stage { get; set; }

        // OPR-14: phiếu ưu tiên được xếp lên đầu hàng chờ
        public bool IsPriority { get; set; }

        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<CounterChange> CounterChanges { get; set; } = new List<CounterChange>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
        [ForeignKey("CounterId")]
        public virtual Counter? Counter { get; set; }
        [ForeignKey("UserId")]
        public virtual Users? Users { get; set; }
    }
}
