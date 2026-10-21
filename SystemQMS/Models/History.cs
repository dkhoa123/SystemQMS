using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class History
    {
        public int notiId { get; set; }
        
        public int guestId { get; set; }
        [MaxLength(100)]
        public string status { get; set; }

        [ForeignKey("guestId")]
        public virtual Guest? guest { get; set; }
        [ForeignKey("notiId")]
        public virtual Notification? notification { get; set; }
    }
}
