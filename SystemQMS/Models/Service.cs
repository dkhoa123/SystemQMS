using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Formats.Asn1;

namespace SystemQMS.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string ServiceId { get; set; }
        [MaxLength(100)]
        public string ServiceName { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public byte[]? Image { get; set; }
        public bool IsActive { get; set; }
        public string SharingLink { get; set; }
        public int AverageTime { get; set; }

        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Tickets> Tickets { get; set; } = new List<Tickets>();
        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public ICollection<SheduleActive> SheduleActives { get; set; } = new List<SheduleActive>();

        public ICollection<NumberFomular> NumberFomulars { get; set; } = new List<NumberFomular>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
