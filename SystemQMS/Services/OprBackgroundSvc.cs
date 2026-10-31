using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    // Job nền cho OPR-07:
    //  - Quét CounterChange Pending quá hạn Opr:TransferTimeoutSeconds -> tự nhận (AutoAccepted)
    //  - Qua 00:00 -> mở lại các quầy đang ngưng nhận phiếu
    public class OprBackgroundSvc : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OprBackgroundSvc> _logger;
        private readonly int _transferTimeoutSeconds;
        private readonly int _scanIntervalSeconds;
        private DateTime _lastDay;

        public OprBackgroundSvc(
            IServiceScopeFactory scopeFactory,
            ILogger<OprBackgroundSvc> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _transferTimeoutSeconds = configuration.GetValue("Opr:TransferTimeoutSeconds", 120);
            _scanIntervalSeconds = Math.Max(1, configuration.GetValue("Opr:ScanIntervalSeconds", 15));
            _lastDay = DateTime.Today;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "OPR background service khởi động: timeout={Timeout}s, chu kỳ quét={Interval}s",
                _transferTimeoutSeconds, _scanIntervalSeconds);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_scanIntervalSeconds));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Không để 1 vòng lỗi giết cả job
                    _logger.LogError(ex, "Lỗi khi chạy job OPR");
                }

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            await AutoAcceptExpiredTransfersAsync(context, ct);
            await ResumePausedCountersOnNewDayAsync(context, ct);
        }

        // CounterChange Pending quá timeout -> hệ thống tự nhận
        private async Task AutoAcceptExpiredTransfersAsync(MyDbContext context, CancellationToken ct)
        {
            // Npgsql không dịch được DateTimeOffset.Now trong mệnh đề so sánh -> dùng UtcNow.
            // createTime lưu dạng DateTimeOffset nên so sánh vẫn đúng theo mốc tuyệt đối.
            var deadline = DateTimeOffset.UtcNow.AddSeconds(-_transferTimeoutSeconds);
            var expired = await context.counterChanges
                .Where(cc => cc.status == CounterChangeStatus.Pending && cc.createTime <= deadline)
                .ToListAsync(ct);

            if (expired.Count == 0)
                return;

            foreach (var change in expired)
            {
                try
                {
                    await TransferSvc.ApplyAcceptAsync(context, change, isAuto: true, ct);
                    _logger.LogInformation(
                        "Tự động nhận chuyển quầy sau {Timeout}s: TransferId={TransferId}, SttId={SttId}, To={To}",
                        _transferTimeoutSeconds, change.id, change.sttId, change.counterIdReceive);
                }
                catch (KeyNotFoundException ex)
                {
                    // Phiếu không còn tồn tại: đóng yêu cầu để không quét lại mãi
                    _logger.LogWarning(ex,
                        "Không tự nhận được chuyển quầy: TransferId={TransferId}, SttId={SttId}",
                        change.id, change.sttId);
                    change.status = CounterChangeStatus.Rejected;
                    await context.SaveChangesAsync(ct);
                }
            }
        }

        // Qua ngày mới: quầy đang ngưng nhận phiếu được mở lại
        private async Task ResumePausedCountersOnNewDayAsync(MyDbContext context, CancellationToken ct)
        {
            var today = DateTime.Today;
            if (today <= _lastDay)
                return;

            _lastDay = today;

            var paused = await context.counters
                .Where(c => c.status == CounterStatus.PausedAccepting)
                .ToListAsync(ct);
            if (paused.Count == 0)
                return;

            foreach (var counter in paused)
            {
                bool hasProcessing = await context.stickers.AnyAsync(
                    s => s.counterId == counter.id && s.status == StickerStatus.Processing, ct);
                counter.status = hasProcessing ? CounterStatus.Serving : CounterStatus.Idle;
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Qua ngày mới, mở lại {Count} quầy đang ngưng nhận phiếu", paused.Count);
        }
    }
}
