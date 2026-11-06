using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Device
    {
        public int id { get; set; }
        public int unitId { get; set; }
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
        public string? ipAddress { get; set; }
        public int volume { get; set; }
        public bool show { get; set; }

        // Mã đăng nhập 6 số
        public string? loginCodeHash { get; set; }
        public DateTime? loginCodeExpiresAt { get; set; }

        // Phiên đăng nhập hiện tại
        public string? sessionTokenHash { get; set; }
        public DateTime? sessionExpiresAt { get; set; }

        [ForeignKey("unitId")]
        public virtual Unit? Unit { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
    }
}
