using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.DspDto
{
    public class ServiceInCounterDto
    {
        public int id { get; set; }
        public string serviceId { get; set; } = "";
        public string serviceName { get; set; } = "";
    }
    public class CounterResponse
    {
        public string id { get; set; }
        public string? name { get; set; }
        public string? deviceCode { get; set; }
        public List<string>? services { get; set; } = new();
        public string? inUse { get; set; }
    }
    public class CreateCounterRequest
    {
        [StringLength(100)]
        public string counterId { get; set; } = "";
        public int unitId { get; set; }
        [StringLength(100)]
        public string? counterName { get; set; }
        [StringLength(255)]
        public string? description { get; set; }
        public bool isActive { get; set; } = true;
        public List<int> serviceIds { get; set; } = new();
    }
    public class UpdateCounterRequest
    {
        [StringLength(100)]
        public string? counterName { get; set; }
        [StringLength(255)]
        public string? description { get; set; }
        public bool isActive { get; set; } = true;
        public List<int> serviceIds { get; set; } = new();
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
