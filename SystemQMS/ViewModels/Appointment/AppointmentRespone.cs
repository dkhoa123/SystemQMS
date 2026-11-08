namespace SystemQMS.ViewModels.Appointment
{
    public class AppointmentResponse
    {
        public int Id { get; set; }

        public int GuestId { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public DateTimeOffset Date { get; set; }

        public DateTimeOffset TimeDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? SttId { get; set; }
    }
}