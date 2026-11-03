using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Ticket
    {
        [Key]
        public int id { get; set; }

        [MaxLength(50)]
        public string ticketNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string status { get; set; } = string.Empty;

        public int serviceId { get; set; }

        public int? CounterId { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        public DateTime? usedAt { get; set; }

        [MaxLength(100)]
        public string? handledBy { get; set; }

        [ForeignKey("CounterId")]
        public virtual Counter? Counter { get; set; }
    }
}