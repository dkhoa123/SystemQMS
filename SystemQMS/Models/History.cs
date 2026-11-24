using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class History
    {
        public int NotiId { get; set; }
        
        public int GuestId { get; set; }
        [MaxLength(100)]
        public string Status { get; set; }

        [ForeignKey("GuestId")]
        public virtual Guest? Guest { get; set; }
        [ForeignKey("NotiId")]
        public virtual Notification? Notification { get; set; }
    }
}
