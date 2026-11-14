using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int GuestId { get; set; }
        
        public int ServiceId { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset TimeDate { get; set; }
        [MaxLength(100)]
        public string? Status { get; set; }
        [MaxLength(100)]
        public string SttId { get; set; }

        [ForeignKey("GuestId")]
        public virtual Guest? Guest { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
        [ForeignKey("SttId")]
        public virtual Ticket? Ticket { get; set; }
    }
}
