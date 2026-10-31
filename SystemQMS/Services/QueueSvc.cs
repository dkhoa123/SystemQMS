using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.QueueDto;

namespace SystemQMS.Services
{
    public class QueueSvc : IQueueSvc
    {
        private const int MaxPageSize = 100;

        private readonly MyDbContext _context;
        private readonly ILogger<QueueSvc> _logger;

        public QueueSvc(MyDbContext context, ILogger<QueueSvc> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Dùng chung cho mọi truy vấn danh sách phiếu, giữ projection dịch được sang SQL
        private static readonly Expression<Func<Sticker, QueueTicketDto>> TicketProjection = s => new QueueTicketDto
        {
            sttId = s.sttId,
            status = s.status,
            stage = s.stage,
            isPriority = s.isPriority,
            issuanceTime = s.issuanceTime,
            useTime = s.useTime,
            serviceName = s.Service != null ? s.Service.serviceName : null,
            handlerName = s.Users != null ? s.Users.username : null
        };

        private async Task<Counter> GetCounterAsync(int counterId, CancellationToken ct)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId, ct);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");
            return counter;
        }

        private async Task<CounterStateResponse> BuildStateAsync(Counter counter, CancellationToken ct)
        {
            var waitingCount = await _context.stickers.CountAsync(
                s => s.counterId == counter.id
                     && s.status == StickerStatus.Waiting
                     && s.stage != StickerStage.TransferPending, ct);
            var processingCount = await _context.stickers.CountAsync(
                s => s.counterId == counter.id && s.status == StickerStatus.Processing, ct);

            return new CounterStateResponse
            {
                counterId = counter.id,
                status = counter.status,
                isAccepting = counter.status != CounterStatus.PausedAccepting
                              && counter.status != CounterStatus.Closed,
                waitingCount = waitingCount,
                processingCount = processingCount
            };
        }

        // OPR-09: ngưng nhận phiếu mới
        public async Task<CounterStateResponse> PauseAcceptingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.status == CounterStatus.Closed)
                throw new ArgumentException("Quầy đã đóng, không thể ngưng nhận phiếu");
            if (counter.status == CounterStatus.PausedAccepting)
                throw new ArgumentException("Quầy đang ngưng nhận phiếu");

            counter.status = CounterStatus.PausedAccepting;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Quầy ngưng nhận phiếu: CounterId={CounterId}", counterId);
            return await BuildStateAsync(counter, ct);
        }

        // OPR-09: nhận phiếu lại
        public async Task<CounterStateResponse> ResumeAcceptingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.status != CounterStatus.PausedAccepting)
                throw new ArgumentException("Quầy không ở trạng thái ngưng nhận phiếu");

            await ResumeCoreAsync(counter, ct);
            _logger.LogInformation("Quầy nhận phiếu lại: CounterId={CounterId}, Status={Status}",
                counterId, counter.status);
            return await BuildStateAsync(counter, ct);
        }

        // OPR-09: 1 endpoint bật/tắt theo tài liệu của thầy
        public async Task<CounterStateResponse> ToggleReceivingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.status == CounterStatus.Closed)
                throw new ArgumentException("Quầy đã đóng, không thể đổi trạng thái nhận phiếu");

            if (counter.status == CounterStatus.PausedAccepting)
                await ResumeCoreAsync(counter, ct);
            else
            {
                counter.status = CounterStatus.PausedAccepting;
                await _context.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Đổi trạng thái nhận phiếu: CounterId={CounterId}, Status={Status}",
                counterId, counter.status);
            return await BuildStateAsync(counter, ct);
        }

        // Còn phiếu đang xử lý thì về Serving, hết thì về Idle
        private async Task ResumeCoreAsync(Counter counter, CancellationToken ct)
        {
            bool hasProcessing = await _context.stickers.AnyAsync(
                s => s.counterId == counter.id && s.status == StickerStatus.Processing, ct);
            counter.status = hasProcessing ? CounterStatus.Serving : CounterStatus.Idle;
            await _context.SaveChangesAsync(ct);
        }

        // OPR-14: đặt ưu tiên, phiếu ưu tiên được xếp lên đầu hàng chờ (Quy tắc 2)
        public async Task<QueueTicketDto> SetPriorityAsync(string sttId, CancellationToken ct = default)
        {
            var sticker = await _context.stickers
                .Include(s => s.Service)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.sttId == sttId, ct);

            if (sticker == null)
                throw new KeyNotFoundException("Phiếu không tồn tại");
            if (sticker.status != StickerStatus.Waiting)
                throw new ArgumentException("Chỉ đặt ưu tiên cho phiếu đang chờ");
            if (sticker.stage == StickerStage.TransferPending)
                throw new ArgumentException("Phiếu đang chờ quầy khác phản hồi, không thể đặt ưu tiên");

            sticker.isPriority = true;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Đặt ưu tiên phiếu: SttId={SttId}", sttId);

            return new QueueTicketDto
            {
                sttId = sticker.sttId,
                status = sticker.status,
                stage = sticker.stage,
                isPriority = sticker.isPriority,
                issuanceTime = sticker.issuanceTime,
                useTime = sticker.useTime,
                serviceName = sticker.Service?.serviceName,
                handlerName = sticker.Users?.username
            };
        }

        // OPR-12: 3 nhóm phiếu của quầy
        public async Task<CounterTicketsResponse> GetCounterTicketsAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await _context.counters.AsNoTracking()
                .FirstOrDefaultAsync(c => c.id == counterId, ct);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            var baseQuery = _context.stickers.AsNoTracking().Where(s => s.counterId == counterId);

            // Quy tắc 2: phiếu ưu tiên lên đầu, cùng mức ưu tiên thì ai lấy số trước đứng trước.
            // Phiếu đang chờ quầy khác phản hồi (TransferPending) không nằm trong hàng chờ.
            var waiting = await baseQuery
                .Where(s => s.status == StickerStatus.Waiting && s.stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.isPriority)
                .ThenBy(s => s.issuanceTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            var processing = await baseQuery
                .Where(s => s.status == StickerStatus.Processing)
                .OrderBy(s => s.issuanceTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            var absent = await baseQuery
                .Where(s => s.status == StickerStatus.Absent)
                .OrderByDescending(s => s.useTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            return new CounterTicketsResponse
            {
                counterId = counter.id,
                counterStatus = counter.status,
                waiting = waiting,
                processing = processing,
                absent = absent
            };
        }

        // OPR-13: gộp phiếu kết thúc (theo useTime) và phiếu đã chuyển đi
        // (CounterChange Accepted/AutoAccepted theo createTime), sort giảm dần rồi mới phân trang.
        public async Task<CounterHistoryResponse> GetCounterHistoryAsync(
            int counterId, int page = 1, int pageSize = 20,
            DateTimeOffset? from = null, DateTimeOffset? to = null,
            CancellationToken ct = default)
        {
            if (!await _context.counters.AnyAsync(c => c.id == counterId, ct))
                throw new KeyNotFoundException("Quầy không tồn tại");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            // Npgsql chỉ nhận tham số DateTimeOffset offset 0 (UTC), trong khi query string
            // bind ra DateTimeOffset theo offset của server (+07:00) -> phải quy về UTC trước,
            // nếu không sẽ lỗi "Cannot write DateTimeOffset with Offset=07:00:00".
            if (from.HasValue) from = from.Value.ToUniversalTime();
            if (to.HasValue) to = to.Value.ToUniversalTime();

            var finishedQuery = _context.stickers.AsNoTracking()
                .Where(s => s.counterId == counterId
                            && (s.status == StickerStatus.Completed
                                || s.status == StickerStatus.Absent
                                || s.status == StickerStatus.Cancelled
                                || s.status == StickerStatus.Expired));
            if (from.HasValue) finishedQuery = finishedQuery.Where(s => s.useTime >= from.Value);
            if (to.HasValue) finishedQuery = finishedQuery.Where(s => s.useTime <= to.Value);

            var finished = await finishedQuery
                .Select(s => new HistoryItemResponse
                {
                    sttId = s.sttId,
                    result = s.status,
                    time = s.useTime,
                    serviceName = s.Service != null ? s.Service.serviceName : null,
                    handlerName = s.Users != null ? s.Users.username : null,
                    isPriority = s.isPriority
                })
                .ToListAsync(ct);

            // Quầy gửi nhìn thấy phiếu mình đã chuyển đi với kết quả "Transferred"
            var transferredQuery = _context.counterChanges.AsNoTracking()
                .Where(cc => cc.counterIdSender == counterId
                             && (cc.status == CounterChangeStatus.Accepted
                                 || cc.status == CounterChangeStatus.AutoAccepted));
            if (from.HasValue) transferredQuery = transferredQuery.Where(cc => cc.createTime >= from.Value);
            if (to.HasValue) transferredQuery = transferredQuery.Where(cc => cc.createTime <= to.Value);

            var transferred = await transferredQuery
                .Select(cc => new HistoryItemResponse
                {
                    sttId = cc.sttId,
                    result = HistoryResult.Transferred,
                    time = cc.createTime,
                    serviceName = cc.Sticker != null && cc.Sticker.Service != null
                        ? cc.Sticker.Service.serviceName
                        : null,
                    handlerName = cc.Sticker != null && cc.Sticker.Users != null
                        ? cc.Sticker.Users.username
                        : null,
                    counterName = cc.CounterReceiver != null ? cc.CounterReceiver.counterName : null
                })
                .ToListAsync(ct);

            var all = finished
                .Concat(transferred)
                .OrderByDescending(x => x.time)
                .ToList();

            return new CounterHistoryResponse
            {
                page = page,
                pageSize = pageSize,
                total = all.Count,
                items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };
        }
    }
}
