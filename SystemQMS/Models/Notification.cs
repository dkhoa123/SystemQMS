using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string type { get; set; }
        [MaxLength(100)]
        public string title { get; set; }
        [MaxLength(255)]
        public string content { get; set; }
        [MaxLength(100)]
        public string form { get; set; }
        public DateTimeOffset timeSend { get; set; }
        [MaxLength(100)]
        public string status { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }

        public ICollection<History> Histories { get; set; } = new List<History>();
    }
}
