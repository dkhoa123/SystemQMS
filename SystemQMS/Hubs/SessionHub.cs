using Microsoft.AspNetCore.SignalR;
using SystemQMS.Models;
using SystemQMS.Services;

namespace SystemQMS.Hubs
{
    public class SessionHub : Hub
    {
        private readonly SessionService _sessionService;
        private readonly KeypadConnectionService _keypadService;

        public SessionHub(
            SessionService sessionService,
            KeypadConnectionService keypadConnection)
        {
            _sessionService = sessionService;
            _keypadService = keypadConnection;
        }

        public async Task RegisterSession(int userId, string deviceType)
        {
            if (deviceType != "CMS" && deviceType != "Keypad")
            {
                throw new HubException("Loại thiết bị không hợp lệ.");
            }

            var oldSession = _sessionService.SetSession(
                userId,
                deviceType,
                Context.ConnectionId);

            if (oldSession != null)
            {
                string message = oldSession.DeviceType == "CMS"
                    ? "Phiên làm việc đang thực hiện trên thiết bị quản lý"
                    : "Phiên làm việc đang thực hiện trên thiết bị phím bấm";

                await Clients.Client(oldSession.ConnectionId)
                    .SendAsync("SessionReplaced", message);
            }
        }

        // CMS gọi hàm này khi kết nối để lấy danh sách quầy đang có Keypad.
        public int[] GetConnectedCounters()
        {
            return _keypadService.GetConnectedCounters();
        }

        public async Task AccessCounter(int counterId)
        {
            if (counterId <= 0)
            {
                throw new HubException("Mã quầy không hợp lệ.");
            }

            _keypadService.Connect(counterId, Context.ConnectionId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"Counter_{counterId}");

            await Clients.All.SendAsync(
                "KeypadConnectionChanged",
                counterId,
                true);
        }

        public async Task LeaveCounter(int counterId)
        {
            if (counterId <= 0)
            {
                throw new HubException("Mã quầy không hợp lệ.");
            }

            // Chỉ ngắt nếu kết nối hiện tại đang giữ quầy này.
            if (_keypadService.GetCounterByConnection(Context.ConnectionId)
                == counterId)
            {
                _keypadService.Disconnect(
                    counterId,
                    Context.ConnectionId);

                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"Counter_{counterId}");

                await Clients.All.SendAsync(
                    "KeypadConnectionChanged",
                    counterId,
                    false);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var session = _sessionService.GetSessionByConnection(
                Context.ConnectionId);

            if (session != null)
            {
                _sessionService.RemoveSession(
                    session.UserId,
                    Context.ConnectionId);
            }

            var counterId = _keypadService.GetCounterByConnection(
                Context.ConnectionId);

            if (counterId.HasValue)
            {
                _keypadService.Disconnect(
                    counterId.Value,
                    Context.ConnectionId);

                await Clients.All.SendAsync(
                    "KeypadConnectionChanged",
                    counterId.Value,
                    false);
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task NotifyNumbersCalled(
            int counterId,
            List<Sticker> stickers)
        {
            await Clients.All.SendAsync(
                "NumbersCalled",
                new
                {
                    counterId,
                    stickers,
                    calledAt = DateTimeOffset.Now
                });
        }



        public async Task NotifyNumbersReset(List<string> sttIds)
        {
            await Clients.All.SendAsync(
                "NumbersReset",
                new
                {
                    sttIds,
                    reason = "Reset quy tắc cấp số",
                    resetAt = DateTimeOffset.UtcNow
                });
        }



        public async Task NotifyNumberCreated(Sticker sticker)
        {
            await Clients.Group($"Counter_{sticker.counterId}")
                .SendAsync("NumberCreated", new
                {
                    sttId = sticker.sttId,
                    serviceId = sticker.serviceId,
                    counterId = sticker.counterId,
                    status = sticker.status,
                    issuanceTime = sticker.issuanceTime
                });
        }
    }
}