using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Counter
    {
        [Key]
        public int id { get; set; }
        [MaxLength(100)]
        public string counterId { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string? counterName { get; set; }
        [MaxLength(255)]
        public string? description { get; set; }
        public bool isActive { get; set; }
        [MaxLength(100)]
        public string status { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<CounterChange> CounterSend { get; set; } = new List<CounterChange>();
        public ICollection<CounterChange> CounterReceive { get; set; } = new List<CounterChange>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Sticker> Stickers { get; set; } = new List<Sticker>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<Users> Users { get; set; } = new List<Users>();

    }
}
