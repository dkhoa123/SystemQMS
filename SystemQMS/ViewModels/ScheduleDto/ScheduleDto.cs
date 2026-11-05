namespace SystemQMS.ViewModels.ScheduleDto
{
    // Một ca làm việc của dịch vụ (bảng SheduleActive)
    public class ShiftDto
    {
        public string wordShift { get; set; } = "";
        public string startTime { get; set; } = "";
        public string endTime { get; set; } = "";
    }

    // Kết quả kiểm tra dịch vụ có đang phục vụ được không (HRS-01)
    public class ServiceAvailabilityResponse
    {
        public int serviceId { get; set; }
        public string serviceName { get; set; } = "";
        public bool inWorkingHours { get; set; }
        public bool hasOpenCounter { get; set; }
        public bool isAvailable { get; set; }


        // null | "OutOfHours" | "NoOpenCounter"
        public string? reason { get; set; }
        public string? currentShift { get; set; }
        public List<ShiftDto> shifts { get; set; } = new();
    }
}