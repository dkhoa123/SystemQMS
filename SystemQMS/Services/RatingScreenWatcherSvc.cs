using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    // RTG-11 (tầng 1): job nền quét thay đổi ở từng quầy và đẩy màn hình đánh giá
    // mới xuống thiết bị qua SignalR — cùng pattern với OprBackgroundSvc.
    // Không đụng vào CounterTicketSvc/CounterSessionSvc của module khác:
    // ai đổi trạng thái quầy hay người xử lý thì tối đa ScanInterval sau thiết bị nhận được.
    public class RatingScreenWatcherSvc : BackgroundService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(3);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<QmsHub> _hubContext;
        private readonly ILogger<RatingScreenWatcherSvc> _logger;

        // "Chữ ký" màn hình lần cuối đã gửi cho từng quầy (sttId + người xử lý + trạng thái quầy)
        private readonly Dictionary<int, string> _lastSignatures = new();

        public RatingScreenWatcherSvc(
            IServiceScopeFactory scopeFactory,
            IHubContext<QmsHub> hubContext,
            ILogger<RatingScreenWatcherSvc> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(ScanInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ScanOnceAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "RatingScreenWatcher: lỗi khi quét thay đổi màn hình đánh giá");
                }
            }
        }

        private async Task ScanOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var evaluationSvc = scope.ServiceProvider.GetRequiredService<IEvaluationSvc>();

            var counterIds = await db.counters.AsNoTracking().Select(c => c.id).ToListAsync(ct);
            // Dọn chữ ký của quầy đã bị xóa, tránh Dictionary phình vô hạn
            foreach (var staleId in _lastSignatures.Keys.Where(id => !counterIds.Contains(id)).ToList())
                _lastSignatures.Remove(staleId);
            foreach (var counterId in counterIds)
            {
                var screen = await evaluationSvc.GetDeviceScreenAsync(counterId, null, ct);
                var signature = $"{screen.currentSttId}|{screen.handlerUserId}|{screen.counterStatus}";

                if (!_lastSignatures.TryGetValue(counterId, out var last))
                {
                    // Lần đầu thấy quầy: chỉ ghi mốc, không bắn (tránh spam khi app khởi động)
                    _lastSignatures[counterId] = signature;
                    continue;
                }
                if (last == signature) continue;

                _lastSignatures[counterId] = signature;
                await _hubContext.Clients.Group($"Counter_{counterId}")
                    .SendAsync("OnRatingContextChanged", screen, ct);
                _logger.LogInformation(
                    "RatingScreenWatcher: quầy {CounterId} đổi màn hình đánh giá ({Signature})",
                    counterId, signature);
            }
        }
    }
}
