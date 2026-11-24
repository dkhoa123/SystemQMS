using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string GuestId { get; set; }
        [MaxLength(100)]
        public string? GuestName { get; set; }
        [MaxLength(100)]
        public string? PhoneNumber { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(100)]
        public string CCCD { get; set; }
        [MaxLength(100)]
        public string Password { get; set; }
        public DateTimeOffset RegisterTime { get; set; }
        [MaxLength(100)]
        public string? Status { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }


        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<History> Histories { get; set; } = new List<History>();  
    }
}
