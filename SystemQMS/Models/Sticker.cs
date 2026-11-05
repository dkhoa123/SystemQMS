using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Sticker
    {
        [Key]
        [MaxLength(100)]
        public string sttId { get; set; }
        public int unitId { get; set; }
        public int serviceId { get; set; }
        public int counterId { get; set; }
        public int userId { get; set; }
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset useTime { get; set; }
        [MaxLength(100)]
        public string status { get; set; }
        [MaxLength(100)]
        public string stage { get; set; }

        // OPR-14: phiếu ưu tiên được xếp lên đầu hàng chờ
        public bool isPriority { get; set; }

        public ICollection<Evaluation> Evaluations = new List<Evaluation>();
        public ICollection<CounterChange> CounterChanges = new List<CounterChange>();
        public ICollection<Booking> Bookings = new List<Booking>();
        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
        [ForeignKey("serviceId")]
        public virtual Service? Service { get; set; }
        [ForeignKey("counterId")]
        public virtual Counter? Counter { get; set; }
        [ForeignKey("userId")]
        public virtual Users? Users { get; set; }
    }
}
