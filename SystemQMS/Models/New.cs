using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class News
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(255)]
        public string AttachmentUrl { get; set; }

        public DateTimeOffset PublishDate { get; set; }

        [MaxLength(100)]
        public string Status { get; set; }

        [Required]
        public int UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }

        [Required]
        public int CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public virtual Users CreatedBy { get; set; }

        public int? UpdatedById { get; set; }
        [ForeignKey("UpdatedById")]
        public virtual Users UpdatedBy { get; set; }
    }
}