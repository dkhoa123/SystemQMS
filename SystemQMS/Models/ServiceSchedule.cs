using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class ServiceSchedule
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        [MaxLength(20)]
        public string DayOfWeek { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public virtual Service? Service { get; set; }

        public ICollection<ServiceScheduleShift> Shifts { get; set; }
            = new List<ServiceScheduleShift>();
    }
}
