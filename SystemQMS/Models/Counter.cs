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
        public string Status { get; set; }

        public ICollection<Service> services = new List<Service>();
        [ForeignKey("unitId")]
        public virtual Unit? unit { get; set; }
        public ICollection<CounterChange> counterSend = new List<CounterChange>();
        public ICollection<CounterChange> counterReceive = new List<CounterChange>();
        public ICollection<Device> devices = new List<Device>();
        public ICollection<Service> service = new List<Service>();
        public ICollection<Sticker> stickers = new List<Sticker>();
        public ICollection<Evaluation> evaluations = new List<Evaluation>();
        public ICollection<Users> users = new List<Users>();

    }
}
