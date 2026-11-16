using System.ComponentModel.DataAnnotations;
using SystemQMS.Constants;

namespace SystemQMS.ViewModels.ServiceDto
{
    public class UpdateServiceDto
    {
        public string serviceCode { get; set; } = string.Empty;
        public string serviceName { get; set; } = string.Empty;
        public string? description { get; set; }
        public string? status { get; set; } = "ACTIVE";
        public string? assignmentMethod { get; set; }
        public Dictionary<string, ServiceScheduleDayDto> schedule { get; set; }
            = new();
        public string? imageUrl { get; set; }
    }
}