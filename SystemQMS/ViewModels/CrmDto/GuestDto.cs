namespace SystemQMS.ViewModels.CrmDto
{
    public class GuestDto
    {
        public int id { get; set; }
        public int unitId { get; set; }
        public string? unitName { get; set; }
        public string guestId { get; set; } = string.Empty;
        public string? guestName { get; set; }
        public string? phoneNumber { get; set; }
        public string? email { get; set; }
        public string CCCD { get; set; } = string.Empty;
        public DateTimeOffset registerTime { get; set; }
        public string? status { get; set; }
    }
}