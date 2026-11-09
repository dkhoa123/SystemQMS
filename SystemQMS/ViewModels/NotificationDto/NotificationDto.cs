using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.NotificationDto
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Form { get; set; }
        public string Type { get; set; }
        public DateTimeOffset TimeSend { get; set; }
        public string Status { get; set; }
        public string AttachmentUrl { get; set; }
        public List<NotificationHistoryDto> Recipients { get; set; } = new();
    }

    public class NotificationHistoryDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string Status { get; set; }
    }

    public class NotificationCreateDto
    {
        [Required, MinLength(1)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Form { get; set; }

        [Required]
        public string Type { get; set; }

        public DateTimeOffset TimeSend { get; set; }

        public string AttachmentUrl { get; set; }

        [Required, MinLength(1, ErrorMessage = "Cần ít nhất 1 người nhận")]
        public List<int> GuestIds { get; set; }
    }

    public class NotificationUpdateDto
    {
        [Required, MinLength(1)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTimeOffset TimeSend { get; set; }
        public string Type { get; set; }
    }
}