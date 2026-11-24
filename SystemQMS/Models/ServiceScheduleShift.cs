using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class ServiceScheduleShift
    {
        public int Id { get; set; }

        public int ScheduleId { get; set; }

        public int SheduleActiveId { get; set; }

        [ForeignKey(nameof(ScheduleId))]
        public virtual ServiceSchedule? Schedule { get; set; }

        [ForeignKey(nameof(SheduleActiveId))]
        public virtual SheduleActive? SheduleActive { get; set; }
    }
}
