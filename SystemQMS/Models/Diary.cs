using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Diary
    {
        public int id { get; set; }
        public int unitId { get; set; }
        public int userId { get; set; }
        [MaxLength(100)]
        public string moduleActive { get; set; }
        [MaxLength(100)]
        public string typeActive { get; set; }
        public int changeNumber { get; set; }
        public DateTimeOffset time { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }
        [ForeignKey("userId")]
        public virtual Users? User { get; set; }
    }
}
