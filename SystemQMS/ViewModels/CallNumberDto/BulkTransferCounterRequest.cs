namespace SystemQMS.ViewModels.CallNumberDto
{
    public class BulkTransferCounterRequest
    {
        public List<string> SttIds { get; set; } = new();
        public int TargetCounterId { get; set; }
    }
}
