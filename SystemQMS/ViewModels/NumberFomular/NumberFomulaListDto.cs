namespace SystemQMS.ViewModels.NumberFomular
{
    public class NumberFomulaListDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string NumberType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int StartNumber { get; set; }
        public int EndNumber { get; set; }
        public bool IsDailyLimitEnabled { get; set; }
        public int? DailyEndNumber { get; set; }
        public DateTimeOffset LimitDay { get; set; }
        public string SequenceMode { get; set; }
        public string FormatSource { get; set; }
    }
}