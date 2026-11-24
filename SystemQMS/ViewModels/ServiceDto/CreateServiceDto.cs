using System.ComponentModel.DataAnnotations;
using SystemQMS.Constants;

namespace SystemQMS.ViewModels.ServiceDto
{
    public class CreateServiceDto
    {
       public string serviceCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        public string serviceName { get; set; } = string.Empty;
        public string? description { get; set; }
        public string? status { get; set; } = "ACTIVE";
        public string? assignmentMethod { get; set; }
        public List<string> counterIds { get; set; } = new();

        public Dictionary<string, ServiceScheduleDayDto> schedule { get; set; } = new();

        public string? imageUrl { get; set; }
    }
}