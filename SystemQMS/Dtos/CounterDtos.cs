using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemQMS.Dtos
{
    public class ServiceInCounterDto
    {
        public int id { get; set; }
        public string serviceId { get; set; } = "";
        public string serviceName { get; set; } = "";
    }
    public class CounterResponse
    {
        public int id { get; set; }
        public string counterId { get; set; } = "";
        public int unitId { get; set; }
        public string? counterName { get; set; }
        public string? description { get; set; }
        public bool isActive { get; set; }
        public string Status { get; set; } = "";
        public List<ServiceInCounterDto> services { get; set; } = new();
    }
    public class CreateCounterRequest
    {
        public string counterId { get; set; } = "";
        public int unitId { get; set; }
        public string? counterName { get; set; }
        public string? description { get; set; }
        public bool isActive { get; set; } = true;
        public List<int> serviceIds { get; set; } = new();
    }
    public class UpdateCounterRequest
    {
        public string? counterName { get; set; }
        public string? description { get; set; }
        public bool isActive { get; set; } = true;
        public List<int> serviceIds { get; set; } = new();
    }

    public class CounterDetailResponse : CounterResponse
    {
        public List<string> handlerNames { get; set; } = new();
    }

    public class CounterStickerResponse
    {
        public string sttId { get; set; } = "";
        public string status { get; set; } = "";
        public string stage { get; set; } = "";
        public DateTimeOffset issuanceTime { get; set; }
        public DateTimeOffset useTime { get; set; }
        public string? serviceName { get; set; }
        public string? handlerName { get; set; }
    }

}
