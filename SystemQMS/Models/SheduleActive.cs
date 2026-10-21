using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class SheduleActive
    {
        public int id { get; set; }
        public int serviceId { get; set; }
        [MaxLength(20)]
        public string wordShift { get; set; }
        public DateTimeOffset startTime { get; set; }
        public DateTimeOffset endTime { get; set; }

        [ForeignKey("serviceId")]
        public virtual Service? Service { get; set; }
    }
}
