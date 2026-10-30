using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class CounterChange
    {
        public int id { get; set; }
        public string sttId { get; set; }
        public int counterIdSender { get; set; }
        public int counterIdReceive { get; set; }
        [MaxLength(100)]
        public string status { get; set; }
        public DateTimeOffset createTime { get; set; }

        [ForeignKey("sttId")]
        public virtual Sticker? Sticker { get; set; }
        [ForeignKey("counterIdSender")]
        public virtual Counter? CounterSender { get; set; }
        [ForeignKey("counterIdReceive")]
        public virtual Counter? CounterReceiver { get; set; }
    }
}
