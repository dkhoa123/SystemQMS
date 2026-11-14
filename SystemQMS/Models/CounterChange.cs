using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class CounterChange
    {
        public int Id { get; set; }
        public string SttId { get; set; }
        public int CounterIdSender { get; set; }
        public int CounterIdReceive { get; set; }
        [MaxLength(100)]
        public string Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }

        [ForeignKey("SttId")]
        public virtual Ticket? Tickets { get; set; }
        [ForeignKey("CounterIdSender")]
        public virtual Counter? CounterSender { get; set; }
        [ForeignKey("CounterIdReceive")]
        public virtual Counter? CounterReceiver { get; set; }
    }
}
