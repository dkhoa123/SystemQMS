namespace SystemQMS.ViewModels.IntergrationsDto
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string GuestId { get; set; }
        public string GuestName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ServiceScheduleDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();
    }

    public class ScheduleDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string WordShift { get; set; }
    }

    public class ReserveSlotRequest
    {
        public int GuestId { get; set; }
        public int ServiceId { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset TimeDate { get; set; }
    }

    public class ReserveSlotResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? BookingId { get; set; }
    }
}