using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Counter
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string CounterId { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string? CounterName { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(100)]
        public string Status { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<CounterChange> CounterSend { get; set; } = new List<CounterChange>();
        public ICollection<CounterChange> CounterReceive { get; set; } = new List<CounterChange>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<Users> Users { get; set; } = new List<Users>();

    }
}
