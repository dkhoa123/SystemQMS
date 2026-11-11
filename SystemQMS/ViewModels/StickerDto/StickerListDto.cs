namespace SystemQMS.ViewModels.Sticker
{
    public class StickerListDto
    {
        public string SttId { get; set; } = "";

        public string Status { get; set; } = "";

        public string Handler { get; set; } = "";

        public DateTimeOffset IssuanceTime { get; set; }

        public DateTimeOffset? UseTime { get; set; }

        public string ServiceName { get; set; } = "";

        public string UserName { get; set; } = "";

        public string CounterName { get; set; } = "";

        public string? CancelReason { get; set; }
    }
}