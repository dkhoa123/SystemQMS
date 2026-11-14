using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SystemQMS.Constants;
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
        private static readonly Expression<Func<Ticket, QueueTicketDto>> TicketProjection = s => new QueueTicketDto
        {
            sttId = s.SttId,
            status = s.Status,
            stage = s.Stage,
            isPriority = s.IsPriority,
            issuanceTime = s.IssuanceTime,
            useTime = s.UseTime,
            serviceName = s.Service != null ? s.Service.ServiceName : null,
            handlerName = s.Users != null ? s.Users.Username : null
        };

        private async Task<Counter> GetCounterAsync(int counterId, CancellationToken ct)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId, ct);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");
            return counter;
        }

        private async Task<CounterStateResponse> BuildStateAsync(Counter counter, CancellationToken ct)
        {
            var waitingCount = await _context.Tickets.CountAsync(
                s => s.CounterId == counter.Id
                     && s.Status == StickerStatus.Waiting
                     && s.Stage != StickerStage.TransferPending, ct);
            var processingCount = await _context.Tickets.CountAsync(
                s => s.CounterId == counter.Id && s.Status == StickerStatus.Processing, ct);

            return new CounterStateResponse
            {
                counterId = counter.Id,
                status = counter.Status,
                isAccepting = counter.Status != CounterStatus.PausedAccepting
                              && counter.Status != CounterStatus.Closed,
                waitingCount = waitingCount,
                processingCount = processingCount
            };
        }

        // OPR-09: ngưng nhận phiếu mới
        public async Task<CounterStateResponse> PauseAcceptingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.Status == CounterStatus.Closed)
                throw new ArgumentException("Quầy đã đóng, không thể ngưng nhận phiếu");
            if (counter.Status == CounterStatus.PausedAccepting)
                throw new ArgumentException("Quầy đang ngưng nhận phiếu");

            counter.Status = CounterStatus.PausedAccepting;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Quầy ngưng nhận phiếu: CounterId={CounterId}", counterId);
            return await BuildStateAsync(counter, ct);
        }

        // OPR-09: nhận phiếu lại
        public async Task<CounterStateResponse> ResumeAcceptingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.Status != CounterStatus.PausedAccepting)
                throw new ArgumentException("Quầy không ở trạng thái ngưng nhận phiếu");

            await ResumeCoreAsync(counter, ct);
            _logger.LogInformation("Quầy nhận phiếu lại: CounterId={CounterId}, Status={Status}",
                counterId, counter.Status); 
            return await BuildStateAsync(counter, ct);
        }

        // OPR-09: 1 endpoint bật/tắt theo tài liệu của thầy
        public async Task<CounterStateResponse> ToggleReceivingAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await GetCounterAsync(counterId, ct);

            if (counter.Status == CounterStatus.Closed)
                throw new ArgumentException("Quầy đã đóng, không thể đổi trạng thái nhận phiếu");

            if (counter.Status == CounterStatus.PausedAccepting)
                await ResumeCoreAsync(counter, ct);
            else
            {
                counter.Status = CounterStatus.PausedAccepting;
                await _context.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Đổi trạng thái nhận phiếu: CounterId={CounterId}, Status={Status}",
                counterId, counter.Status);
            return await BuildStateAsync(counter, ct);
        }

        // Còn phiếu đang xử lý thì về Serving, hết thì về Idle
        private async Task ResumeCoreAsync(Counter counter, CancellationToken ct)
        {
            bool hasProcessing = await _context.Tickets.AnyAsync(
                s => s.CounterId == counter.Id && s.Status == StickerStatus.Processing, ct);
            counter.Status = hasProcessing ? CounterStatus.Serving : CounterStatus.Idle;
            await _context.SaveChangesAsync(ct);
        }

        // OPR-14: đặt ưu tiên, phiếu ưu tiên được xếp lên đầu hàng chờ (Quy tắc 2)
        public async Task<QueueTicketDto> SetPriorityAsync(string sttId, CancellationToken ct = default)
        {
            var sticker = await _context.Tickets
                .Include(s => s.Service)
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.SttId == sttId, ct);

            if (sticker == null)
                throw new KeyNotFoundException("Phiếu không tồn tại");
            if (sticker.Status != StickerStatus.Waiting)
                throw new ArgumentException("Chỉ đặt ưu tiên cho phiếu đang chờ");
            if (sticker.Stage == StickerStage.TransferPending)
                throw new ArgumentException("Phiếu đang chờ quầy khác phản hồi, không thể đặt ưu tiên");

            sticker.IsPriority = true;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Đặt ưu tiên phiếu: SttId={SttId}", sttId);

            return new QueueTicketDto
            {
                sttId = sticker.SttId,
                status = sticker.Status,
                stage = sticker.Stage,
                isPriority = sticker.IsPriority,
                issuanceTime = sticker.IssuanceTime,
                useTime = sticker.UseTime,
                serviceName = sticker.Service?.ServiceName,
                handlerName = sticker.Users?.Username
            };
        }

        // OPR-12: 3 nhóm phiếu của quầy
        public async Task<CounterTicketsResponse> GetCounterTicketsAsync(int counterId, CancellationToken ct = default)
        {
            var counter = await _context.Counters.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == counterId, ct);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");

            var baseQuery = _context.Tickets.AsNoTracking().Where(s => s.CounterId == counterId);

            // Quy tắc 2: phiếu ưu tiên lên đầu, cùng mức ưu tiên thì ai lấy số trước đứng trước.
            // Phiếu đang chờ quầy khác phản hồi (TransferPending) không nằm trong hàng chờ.
            var waiting = await baseQuery
                .Where(s => s.Status == StickerStatus.Waiting && s.Stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.IsPriority)
                .ThenBy(s => s.IssuanceTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            var processing = await baseQuery
                .Where(s => s.Status == StickerStatus.Processing)
                .OrderBy(s => s.IssuanceTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            var absent = await baseQuery
                .Where(s => s.Status == StickerStatus.Absent)
                .OrderByDescending(s => s.UseTime)
                .Select(TicketProjection)
                .ToListAsync(ct);

            return new CounterTicketsResponse
            {
                counterId = counter.Id,
                counterStatus = counter.Status,
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
            if (!await _context.Counters.AnyAsync(c => c.Id == counterId, ct))
                throw new KeyNotFoundException("Quầy không tồn tại");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            // Npgsql chỉ nhận tham số DateTimeOffset offset 0 (UTC), trong khi query string
            // bind ra DateTimeOffset theo offset của server (+07:00) -> phải quy về UTC trước,
            // nếu không sẽ lỗi "Cannot write DateTimeOffset with Offset=07:00:00".
            if (from.HasValue) from = from.Value.ToUniversalTime();
            if (to.HasValue) to = to.Value.ToUniversalTime();

            var finishedQuery = _context.Tickets.AsNoTracking()
                .Where(s => s.CounterId == counterId
                            && (s.Status == StickerStatus.Completed
                                || s.Status == StickerStatus.Absent
                                || s.Status == StickerStatus.Cancelled
                                || s.Status == StickerStatus.Expired));
            if (from.HasValue) finishedQuery = finishedQuery.Where(s => s.UseTime >= from.Value);
            if (to.HasValue) finishedQuery = finishedQuery.Where(s => s.UseTime <= to.Value);

            var finished = await finishedQuery
                .Select(s => new HistoryItemResponse
                {
                    sttId = s.SttId,
                    result = s.Status,
                    time = s.UseTime,
                    serviceName = s.Service != null ? s.Service.ServiceName : null,
                    handlerName = s.Users != null ? s.Users.Username : null,
                    isPriority = s.IsPriority
                })
                .ToListAsync(ct);

            // Quầy gửi nhìn thấy phiếu mình đã chuyển đi với kết quả "Transferred"
            var transferredQuery = _context.CounterChanges.AsNoTracking()
                .Where(cc => cc.CounterIdSender == counterId
                             && (cc.Status == CounterChangeStatus.Accepted
                                 || cc.Status == CounterChangeStatus.AutoAccepted));
            if (from.HasValue) transferredQuery = transferredQuery.Where(cc => cc.CreateTime >= from.Value);
            if (to.HasValue) transferredQuery = transferredQuery.Where(cc => cc.CreateTime <= to.Value);

            var transferred = await transferredQuery
                .Select(cc => new HistoryItemResponse
                {
                    sttId = cc.SttId,
                    result = HistoryResult.Transferred,
                    time = cc.CreateTime,
                    serviceName = cc.Tickets != null && cc.Tickets.Service != null
                        ? cc.Tickets.Service.ServiceName
                        : null,
                    handlerName = cc.Tickets != null && cc.Tickets.Users != null
                        ? cc.Tickets.Users.Username
                        : null,
                    counterName = cc.CounterReceiver != null ? cc.CounterReceiver.CounterName : null
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
