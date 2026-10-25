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

        public ICollection<Counter> counters { get; set; } = new List<Counter>();
        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Sticker> Stickers { get; set; } = new List<Sticker>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<SheduleActive> SheduleActives { get; set; } = new List<SheduleActive>();

        public ICollection<NumberFomular> NumberFomulars { get; set; } = new List<NumberFomular>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
