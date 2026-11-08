namespace SystemQMS.ViewModels.Appointment
{
    public class AppointmentRequest
    {
        public int serviceId { get; set; }
        public DateTimeOffset appointmentDate { get; set; }
        public DateTimeOffset appointmentTime { get; set; }
    }
}