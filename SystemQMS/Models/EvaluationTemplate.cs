using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class EvaluationTemplate
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string TemplateId { get; set; }
        [MaxLength(100)]
        public string TemplateName { get; set; }
        [MaxLength(255)]
        public string ContentQuestion { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
    }
}
