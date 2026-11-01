namespace SystemQMS.ViewModels.CallNumberDto
{
    public class TransferCounterRequest
    {
        public string SttId { get; set; } = string.Empty;
        public int TargetCounterId
        {
            get; set;
        }
    }
}