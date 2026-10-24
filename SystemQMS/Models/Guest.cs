using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Guest
    {
        public int id { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string guestId { get; set; }
        [MaxLength(100)]
        public string? guestName { get; set; }
        [MaxLength(100)]
        public string? phoneNumber { get; set; }
        [MaxLength(100)]
        public string? email { get; set; }
        [MaxLength(100)]
        public string CCCD { get; set; }
        [MaxLength(100)]
        public string password { get; set; }
        public DateTimeOffset registerTime { get; set; }
        [MaxLength(100)]
        public string? status { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }


        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<History> Histories { get; set; } = new List<History>();  
    }
}
