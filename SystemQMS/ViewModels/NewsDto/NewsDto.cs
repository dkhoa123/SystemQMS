
using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.NewsDto
{
    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string Status { get; set; }
        public string CreatedByName { get; set; }
    }

    public class NewsCreateDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public string AttachmentUrl { get; set; }

        public string Status { get; set; } = "Bản nháp";
    }

    public class NewsUpdateDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        public string AttachmentUrl { get; set; }

        public string Status { get; set; }
    }
}