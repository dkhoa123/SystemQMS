using SystemQMS.Constants;

namespace SystemQMS.ViewModels.ServiceDto
{
    public class ServiceResponseDto
    {
        public int id { get; set; }

        public string serviceCode { get; set; } = string.Empty;

        public string serviceName { get; set; } = string.Empty;

        public List<string> counterIds { get; set; } = new();

        public string? status { get; set; }

        public DateTime createdAt { get; set; }

        public string? description { get; set; }

        public string? assignmentMethod { get; set; }

        public Dictionary<string, ServiceScheduleDayDto> schedule { get; set; }
            = new();

        public byte[]? imageUrl { get; set; }
    }

    // DTO chi tiết dịch vụ gồm quầy xử lý và danh sách STT đã cấp (SVC-05)

    public class ServiceDetailDto : ServiceResponseDto
    {
        //public List<ServiceCounterItemDto> assignedCounters { get; set; } = new List<ServiceCounterItemDto>();
        //public List<ServiceTicketItemDto> issuedTickets { get; set; } = new List<ServiceTicketItemDto>();
    }

    public class ServiceCounterItemDto
    {
        public int id { get; set; }
        public string counterId { get; set; } = string.Empty;
        public string? counterName { get; set; }
        public bool isActive { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public class ServiceTicketItemDto
    {
        public string id { get; set; } = string.Empty;
        public string ticketNumber { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string stage { get; set; } = string.Empty;
        public string? handlerUser { get; set; }
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset useTime { get; set; }
        public string? counterName { get; set; }
    }
}