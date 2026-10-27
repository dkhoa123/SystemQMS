using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class UnitConfig
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Unit")]
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        [ForeignKey("EvaluationTemplate")]
        public int? ActiveEvaluationTemplateId { get; set; }
        public virtual EvaluationTemplate EvaluationTemplate { get; set; }
        public string BackgroundImage { get; set; }
        public string BannerText { get; set; }
        public string ToneColorHex { get; set; }
        public int? ActiveAudioTemplateId { get; set; }
    }
}
