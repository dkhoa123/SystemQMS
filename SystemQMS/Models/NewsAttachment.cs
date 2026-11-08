
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class NewsAttachment
    {
        public int Id { get; set; }

        public int newsId { get; set; }

        [Required]
        [MaxLength(255)]
        public string fileName { get; set; } = string.Empty;

        [Required]
        public string fileUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? fileType { get; set; }

        [ForeignKey("newsId")]
        public virtual News? News { get; set; }
    }
}
