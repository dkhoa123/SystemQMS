using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Diary
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int UserId { get; set; }
        [MaxLength(100)]
        public string ModuleActive { get; set; }
        [MaxLength(100)]
        public string TypeActive { get; set; }
        public int ChangeNumber { get; set; }
        public DateTimeOffset Time { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
    }
}
