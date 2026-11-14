using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class Device
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        [MaxLength(100)]
        public string DeviceId { get; set; }
        [MaxLength(100)]
        public string DeviceName { get; set; }
        [MaxLength(100)]
        public string DeviceType { get; set; }
        [MaxLength(100)]
        public string StatusActive { get; set; }
        [MaxLength(100)]
        public string StatusConnect { get; set; }
        [MaxLength(100)]
        public string? IpAddress { get; set; }
        public int Volume { get; set; }
        public bool Show { get; set; }

        [MaxLength(100)]
        public string TemplateId { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }

        // Mã đăng nhập 6 số
        public string? LoginCodeHash { get; set; }
        public DateTime? LoginCodeExpiresAt { get; set; }

        // Phiên đăng nhập hiện tại
        public string? SessionTokenHash { get; set; }
        public DateTime? SessionExpiresAt { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit? Unit { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Counter> Counters { get; set; } = new List<Counter>();
    }
}
