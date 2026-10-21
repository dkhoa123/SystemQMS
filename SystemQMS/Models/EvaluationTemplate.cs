using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class EvaluationTemplate
    {
        public int id { get; set; }
        public int unitId { get; set; }
        [MaxLength(100)]
        public string templateId { get; set; }
        [MaxLength(100)]
        public string templateName { get; set; }
        [MaxLength(255)]
        public string contentQuestion { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
    }
}
