using Microsoft.AspNetCore.SignalR;

namespace SystemQMS.Hubs
{
    public class QmsHub : Hub
    {
        // RTG-11: thiết bị đánh giá của một quầy join group riêng của quầy đó
        // để nhận event OnRatingContextChanged khi đổi người xử lý / trạng thái quầy.
        public async Task JoinCounter(int counterId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Counter_{counterId}");
        }

        public async Task LeaveCounter(int counterId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Counter_{counterId}");
        }

        // Màn hình hiển thị của đơn vị: group Unit_{unitId}
        public async Task JoinUnit(int unitId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Unit_{unitId}");
        }

        public async Task LeaveUnit(int unitId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Unit_{unitId}");
        }
    }
}