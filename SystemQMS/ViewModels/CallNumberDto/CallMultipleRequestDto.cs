using System.ComponentModel.DataAnnotations;

namespace SystemQMS.ViewModels.CallNumberDto
{
    public class CallMultipleRequest
    {
        public int CounterId { get; set; }

        [Range(1, 6, ErrorMessage = "Số lượng gọi phải từ 1 đến 6.")]
        public int Quantity { get; set; }
    }
}
