using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Evaluation
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string SttId { get; set; }
        public int ServiceId { get; set; }
        public int CounterId { get; set; }
        [MaxLength(100)]
        public string TemplateId { get; set; }
        public int TimeMark { get; set; }
        public int AttitudeMark { get; set; }
        public int MaterialMark { get; set; }
        public int SatisfedMark { get; set; }
        [MaxLength(100)]
        public string Orther { get; set; }
        [MaxLength(20)]
        public string GuestPhone { get; set; }
        [MaxLength(100)]
        public string GuestName { get; set; }
        public DateTimeOffset EvaluationTime { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        [ForeignKey("SttId")]
        public virtual Tickets? Tickets { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
        [ForeignKey("CounterId")]
        public virtual Counter? Counter { get; set; }
    }
}
