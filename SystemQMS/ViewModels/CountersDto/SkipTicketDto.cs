using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.CountersDto
{
    public class SkipTicketRequestDto
    {
        [MaxLength(255, ErrorMessage = "Ghi chú vắng mặt không vượt quá 255 ký tự")]
        public string? note { get; set; }
    }
}