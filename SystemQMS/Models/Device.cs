using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace SystemQMS.Models
{
    public class Device
    {
        public int id { get; set; }
        [MaxLength(100)]
        public string deviceId { get; set; }
        [MaxLength(100)]
        public string deviceName { get; set; }
        [MaxLength(100)]
        public string deviceType { get; set; }
        [MaxLength(100)]
        public string statusActive { get; set; }
        [MaxLength(100)]
        public string statusConnect { get; set; }
        [MaxLength(100)]
        public string ipAddress { get; set; }
        public byte[]? voice { get; set; }
        public bool show { get; set; }

        public ICollection<Service> services = new List<Service>();
        public ICollection<Counter> counters = new List<Counter>();
    }
}
