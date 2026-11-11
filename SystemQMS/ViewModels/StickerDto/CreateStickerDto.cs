using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.Sticker
{
    public class CreateStickerDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int CounterId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}