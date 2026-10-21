using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Booking
    {
        public int id { get; set; }

        public int guestId { get; set; }
        
        public int serviceId { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset timeDate { get; set; }
        [MaxLength(100)]
        public string status { get; set; }
        [MaxLength(100)]
        public string sttId { get; set; }

        [ForeignKey("guestId")]
        public virtual Guest? Guest { get; set; }
        [ForeignKey("serviceId")]
        public virtual Service? Service { get; set; }
        [ForeignKey("sttId")]
        public virtual Sticker? Sticker { get; set; }
    }
}
