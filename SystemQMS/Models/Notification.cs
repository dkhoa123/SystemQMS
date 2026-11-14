using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string Type { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(255)]
        public string Content { get; set; }
        [MaxLength(100)]
        public string Form { get; set; }
        public DateTimeOffset TimeSend { get; set; }
        [MaxLength(100)]
        public string Status { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        public ICollection<History> Histories { get; set; } = new List<History>();
        [MaxLength(255)]
        public string AttachmentUrl { get; set; }
    }
}
