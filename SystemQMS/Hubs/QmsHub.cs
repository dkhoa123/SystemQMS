using Microsoft.AspNetCore.SignalR;

namespace SystemQMS.Hubs
{
    // Interface định nghĩa các hàm Client sẽ lắng nghe
    public interface IDspHubClient
    {
        // Khi gọi số mới (Kích hoạt Loa, Màn hình quầy, Màn hình sảnh)
        Task ReceiveTicketCall(int counterId, string counterName, List<string> ticketNumbers, string audioTemplate);

        // Khi gọi lại số (Chỉ kích hoạt Loa - DSP-07)
        Task ReceiveTicketRecall(int counterId, string ticketNumber, string audioTemplate);

        // Khi hoàn thành/bỏ qua (Cập nhật màn hình quầy - DSP-04)
        Task ReceiveTicketProcessed(int counterId, string ticketNumber, string status);

        // Khi trạng thái quầy thay đổi (Đóng quầy, ngưng nhận số - DSP-02, DSP-03)
        Task ReceiveCounterStatusChanged(int counterId, string newStatus);

        // Khi hàng chờ thay đổi
        Task ReceiveWaitlistUpdate(int counterId, List<string> newWaitingTickets);
    }

    public class DspHub : Hub<IDspHubClient>
    {
        // Client có thể tham gia vào nhóm theo ID quầy để nhận thông báo riêng
        public async Task SubscribeToCounter(int counterId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Counter_{counterId}");
        }
    }

    public class QmsHub : Hub
    {
    }
}