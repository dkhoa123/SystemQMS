using Microsoft.AspNetCore.Http;

namespace SystemQMS.Dtos
{
    public class DisplayConfigUpdateDto
    {
        public string UnitName { get; set; }
        public string BannerText { get; set; }
        public string ToneColorHex { get; set; } 
        public int? EvaluationTemplateId { get; set; }

        public IFormFile BackgroundFile { get; set; } 
        public IFormFile LogoFile { get; set; }
    }

    public class DisplayConfigResponseDto
    {
        public string UnitName { get; set; }
        public string LogoUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string BannerText { get; set; }
        public string ToneColorHex { get; set; }
        public int? EvaluationTemplateId { get; set; }
    }

    public class AudioConfigUpdateDto
    {
        public int TemplateId { get; set; }
    }
}
