using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemQMS.Models
{
    public class UnitConfig
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Unit")]
        public int UnitId { get; set; }
        public virtual Unit Unit { get; set; }
        [ForeignKey("EvaluationTemplate")]
        public int? ActiveEvaluationTemplateId { get; set; }
        public virtual EvaluationTemplate EvaluationTemplate { get; set; }
        public string BackgroundImage { get; set; }
        public string BannerText { get; set; }
        public string ToneColorHex { get; set; }
        public int? ActiveAudioTemplateId { get; set; }
        // --- Bổ sung nhóm Cấu hình giao diện Web App ---
        [MaxLength(255)]
        public string Favicon { get; set; }

        [MaxLength(255)]
        public string Slogan { get; set; }

        [MaxLength(100)]
        public string Website { get; set; }

        [MaxLength(255)]
        public string TitleHome { get; set; }

        public string DescriptionHome { get; set; }

        [MaxLength(100)]
        public string BookingMethod { get; set; }

        public int? SlotDuration { get; set; }

        public int? MaxGuestsPerSlot { get; set; }

        public int? AdvanceBookingDays { get; set; }

        public bool AllowImmediateBooking { get; set; }

        public int? MaxBookingsPerUserDaily { get; set; }

        [MaxLength(100)]
        public string AppliedScope { get; set; }
    }
}
