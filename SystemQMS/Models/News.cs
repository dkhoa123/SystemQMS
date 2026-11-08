
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class News
    {
        public int Id { get; set; }

        public int unitId { get; set; }

        [Required]
        [MaxLength(200)]
        public string title { get; set; } = string.Empty;

        public string content { get; set; } = string.Empty;

        // Thời gian đăng bài
        public DateTimeOffset timeSend { get; set; }

        // Thời gian hết hiệu lực
        public DateTimeOffset? expiredTime { get; set; }

        // Draft / Published / Hidden
        [MaxLength(50)]
        public string status { get; set; } = "Draft";

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }

        public ICollection<NewsAttachment> Attachments { get; set; }
            = new List<NewsAttachment>();
    }
}
