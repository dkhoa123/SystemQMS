using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Formats.Asn1;

namespace SystemQMS.Models
{
    public class Service
    {
        public int id { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string serviceId { get; set; }
        [MaxLength(100)]
        public string serviceName { get; set; }
        [MaxLength(255)]
        public string? description { get; set; }
        public byte[]? image { get; set; }
        public bool isActive { get; set; }
        public string sharingLink { get; set; }
        public DateTimeOffset averageTime { get; set; }

        public ICollection<Counter> counters = new List<Counter>();
        [ForeignKey("unitId")]
        public virtual Unit? unit { get; set; }
        public ICollection<Device> devices = new List<Device>();
        public ICollection<Sticker> stickers = new List<Sticker>();
        public ICollection<Evaluation> evaluations = new List<Evaluation>();
        public ICollection<SheduleActive> sheduleActives = new List<SheduleActive>();

        public ICollection<NumberFomular> numberFomulars = new List<NumberFomular>();
        public ICollection<Booking> bookings = new List<Booking>();
    }
}
