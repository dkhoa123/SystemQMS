using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class CounterSession
    {
        [Key]
        public int id { get; set; }

        public int counterId { get; set; }
        public int userId { get; set; }
        public int deviceId { get; set; }

        public DateTime startedAt { get; set; }
        public DateTime? endedAt { get; set; }

        public bool isActive { get; set; }

        [ForeignKey(nameof(counterId))]
        public Counter? Counter { get; set; }

        [ForeignKey(nameof(userId))]
        public Users? User { get; set; }

        [ForeignKey(nameof(deviceId))]
        public Device? Device { get; set; }
    }
}