using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.PageResultDto;
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

        //// Dùng chung cho mọi truy vấn danh sách phiếu, giữ projection dịch được sang SQL
        //private static readonly Expression<Func<Tickets, QueueTicketDto>> TicketProjection = s => new QueueTicketDto
        //{
        //    sttId = s.SttId,
        //    status = s.Status,
        //    stage = s.Stage,
        //    isPriority = s.IsPriority,
        //    issuanceTime = s.IssuanceTime,
        //    useTime = s.UseTime,
        //    serviceName = s.Service != null ? s.Service.ServiceName : null,
        //    handlerName = s.Users != null ? s.Users.Username : null
        //};

        private async Task<Counter> GetCounterAsync(int counterId, CancellationToken ct)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId, ct);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");
            return counter;
        }

        public CounterStateResponse getStatesCounter(int counterId)
        {
            var counter = _context.Counters.AsNoTracking()
                   .FirstOrDefault(c => c.Id == counterId);
            if (counter == null)
                throw new KeyNotFoundException("Quầy không tồn tại");
            return new CounterStateResponse
            {
                issued = _context.Tickets.Count(s => s.CounterId == counter.Id && s.Status == "Completed"),
                waiting = _context.Tickets.Count(s => s.CounterId == counter.Id && s.Status == "Waiting"),
                used = _context.Tickets.Count(s => s.CounterId == counter.Id && s.Status == "Used"),
                skipped = _context.Tickets.Count(s => s.CounterId == counter.Id && s.Status == "Skipped"),
                cancelled = _context.Tickets.Count(s => s.CounterId == counter.Id && s.Status == "Cancelled")
            };
        }

        public PageResultDto<QueueTicketDto> GetCounterTickets(FilterQueueDto filter)
        {
            var query = _context.Tickets
                .Include(s => s.Service)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(filter.keyword))
            {
                query = query.Where(s => s.SttId.Contains(filter.keyword) ||
                s.Service.ServiceName.Contains(filter.keyword));
            }
            if (!string.IsNullOrEmpty(filter.status))
            {
                query = query.Where(s => s.Status == filter.status);
            }
            if(filter.startDate.HasValue)
            {
                query = query.Where(s => s.IssuanceTime >= filter.startDate);
            }
            if(filter.endDate.HasValue)
            {
                var endDate = filter.endDate.Value.Date.AddDays(1);

                query = query.Where(s =>
                    s.IssuanceTime < endDate);
            }

            // Đếm tổng số bản ghi sau khi lọc
            int totalRecords = query.Count();

            int page = Math.Max(1, filter.page); // Đảm bảo trang tối thiểu là 1
            int pageSize = Math.Max(1, filter.pageSize); // Đảm bảo kích thước trang tối thiểu là 1

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / filter.pageSize
            );

            var tickets = query
                .OrderByDescending(s => s.IssuanceTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = tickets.Select(s => new QueueTicketDto
            {
                id = s.SttId,
                number = s.TicketNumber,
                issuedAt = s.IssuanceTime.ToString("yyyy-MM-dd HH:mm:ss"),
                serviceName = s.Service != null ? s.Service.ServiceName : null,
                status = s.Stage,
                
            });
            return new PageResultDto<QueueTicketDto>
            {
                items = data.ToList(),
                page = page,
                pageSize = pageSize,
                total = totalPages
            };
        }

        //private async Task<CounterStateResponse> BuildStateAsync(int counterId,)
        //{
        //    var waitingCount = await _context.Tickets.CountAsync(
        //        s => s.CounterId == counter.Id
        //             && s.Status == StickerStatus.Waiting
        //             && s.Stage != StickerStage.TransferPending, ct);
        //    var processingCount = await _context.Tickets.CountAsync(
        //        s => s.CounterId == counter.Id && s.Status == StickerStatus.Processing, ct);

        //    return new CounterStateResponse
        //    {
        //        counterId = counter.Id,
        //        status = counter.Status,
        //        isAccepting = counter.Status != CounterStatus.PausedAccepting
        //                      && counter.Status != CounterStatus.Closed,
        //        waitingCount = waitingCount,
        //        processingCount = processingCount
        //    };
        //}

        //// OPR-09: ngưng nhận phiếu mới
        //public async Task<CounterStateResponse> PauseAcceptingAsync(int counterId, CancellationToken ct = default)
        //{
        //    var counter = await GetCounterAsync(counterId, ct);

        //    if (counter.Status == CounterStatus.Closed)
        //        throw new ArgumentException("Quầy đã đóng, không thể ngưng nhận phiếu");
        //    if (counter.Status == CounterStatus.PausedAccepting)
        //        throw new ArgumentException("Quầy đang ngưng nhận phiếu");

        //    counter.Status = CounterStatus.PausedAccepting;
        //    await _context.SaveChangesAsync(ct);
        //    _logger.LogInformation("Quầy ngưng nhận phiếu: CounterId={CounterId}", counterId);
        //    return await BuildStateAsync(counter, ct);
        //}

        //// OPR-09: nhận phiếu lại
        //public async Task<CounterStateResponse> ResumeAcceptingAsync(int counterId, CancellationToken ct = default)
        //{
        //    var counter = await GetCounterAsync(counterId, ct);

        //    if (counter.Status != CounterStatus.PausedAccepting)
        //        throw new ArgumentException("Quầy không ở trạng thái ngưng nhận phiếu");

        //    await ResumeCoreAsync(counter, ct);
        //    _logger.LogInformation("Quầy nhận phiếu lại: CounterId={CounterId}, Status={Status}",
        //        counterId, counter.Status); 
        //    return await BuildStateAsync(counter, ct);
        //}

        //// OPR-09: 1 endpoint bật/tắt theo tài liệu của thầy
        //public async Task<CounterStateResponse> ToggleReceivingAsync(int counterId, CancellationToken ct = default)
        //{
        //    var counter = await GetCounterAsync(counterId, ct);

        //    if (counter.Status == CounterStatus.Closed)
        //        throw new ArgumentException("Quầy đã đóng, không thể đổi trạng thái nhận phiếu");

        //    if (counter.Status == CounterStatus.PausedAccepting)
        //        await ResumeCoreAsync(counter, ct);
        //    else
        //    {
        //        counter.Status = CounterStatus.PausedAccepting;
        //        await _context.SaveChangesAsync(ct);
        //    }

        //    _logger.LogInformation("Đổi trạng thái nhận phiếu: CounterId={CounterId}, Status={Status}",
        //        counterId, counter.Status);
        //    return await BuildStateAsync(counter, ct);
        //}

        // Còn phiếu đang xử lý thì về Serving, hết thì về Idle
        private async Task ResumeCoreAsync(Counter counter, CancellationToken ct)
        {
            bool hasProcessing = await _context.Tickets.AnyAsync(
                s => s.CounterId == counter.Id && s.Status == StickerStatus.Processing, ct);
            counter.Status = hasProcessing ? CounterStatus.Serving : CounterStatus.Idle;
            await _context.SaveChangesAsync(ct);
        }

        // OPR-14: đặt ưu tiên, phiếu ưu tiên được xếp lên đầu hàng chờ (Quy tắc 2)
        //public async Task<QueueTicketDto> SetPriorityAsync(string sttId, CancellationToken ct = default)
        //{
        //    var sticker = await _context.Tickets
        //        .Include(s => s.Service)
        //        .Include(s => s.Users)
        //        .FirstOrDefaultAsync(s => s.SttId == sttId, ct);

        //    if (sticker == null)
        //        throw new KeyNotFoundException("Phiếu không tồn tại");
        //    if (sticker.Status != StickerStatus.Waiting)
        //        throw new ArgumentException("Chỉ đặt ưu tiên cho phiếu đang chờ");
        //    if (sticker.Stage == StickerStage.TransferPending)
        //        throw new ArgumentException("Phiếu đang chờ quầy khác phản hồi, không thể đặt ưu tiên");

        //    sticker.IsPriority = true;
        //    await _context.SaveChangesAsync(ct);
        //    _logger.LogInformation("Đặt ưu tiên phiếu: SttId={SttId}", sttId);

        //    return new QueueTicketDto
        //    {
        //        sttId = sticker.SttId,
        //        status = sticker.Status,
        //        stage = sticker.Stage,
        //        isPriority = sticker.IsPriority,
        //        issuanceTime = sticker.IssuanceTime,
        //        useTime = sticker.UseTime,
        //        serviceName = sticker.Service?.ServiceName,
        //        handlerName = sticker.Users?.Username
        //    };
        //}

        //// OPR-12: 3 nhóm phiếu của quầy
        //public async Task<CounterTicketsResponse> GetCounterTicketsAsync(int counterId, CancellationToken ct = default)
        //{
        //    var counter = await _context.Counters.AsNoTracking()
        //        .FirstOrDefaultAsync(c => c.Id == counterId, ct);
        //    if (counter == null)
        //        throw new KeyNotFoundException("Quầy không tồn tại");

        //    var baseQuery = _context.Tickets.AsNoTracking().Where(s => s.CounterId == counterId);

        //    // Quy tắc 2: phiếu ưu tiên lên đầu, cùng mức ưu tiên thì ai lấy số trước đứng trước.
        //    // Phiếu đang chờ quầy khác phản hồi (TransferPending) không nằm trong hàng chờ.
        //    var waiting = await baseQuery
        //        .Where(s => s.Status == StickerStatus.Waiting && s.Stage != StickerStage.TransferPending)
        //        .OrderByDescending(s => s.IsPriority)
        //        .ThenBy(s => s.IssuanceTime)
        //        .Select(TicketProjection)
        //        .ToListAsync(ct);

        //    var processing = await baseQuery
        //        .Where(s => s.Status == StickerStatus.Processing)
        //        .OrderBy(s => s.IssuanceTime)
        //        .Select(TicketProjection)
        //        .ToListAsync(ct);

        //    var absent = await baseQuery
        //        .Where(s => s.Status == StickerStatus.Absent)
        //        .OrderByDescending(s => s.UseTime)
        //        .Select(TicketProjection)
        //        .ToListAsync(ct);

        //    return new CounterTicketsResponse
        //    {
        //        counterId = counter.Id,
        //        counterStatus = counter.Status,
        //        waiting = waiting,
        //        processing = processing,
        //        absent = absent
        //    };
        //}

        // OPR-13: gộp phiếu kết thúc (theo useTime) và phiếu đã chuyển đi
        // (CounterChange Accepted/AutoAccepted theo createTime), sort giảm dần rồi mới phân trang.



        public List<HistoryItemResponse> GetCounterHistory(int counterId)
        {
            var targetStatuses = new[] { "DONE", "TRANSFERRED", "ABSENT" };
            var sticker = _context.Tickets
        .AsNoTracking()
        .Where(s => s.CounterId == counterId)
        .OrderByDescending(s => s.UseTime)
        .Select(s => new HistoryItemResponse
        {
            id = s.SttId,
            number = s.TicketNumber,
            serviceName = s.Service != null ? s.Service.ServiceName : null,
            staffName = s.Users != null ? s.Users.Username : null,
            result = s.Counter.Status
        })
        .ToList();
            return sticker;
        }

        private const string StageWaiting = "WAITING_ASSIGN";
        private const string StageProcessing = "WAITING_ACCEPT";


        // Gọi đúng một vé theo SttId
        public QueueTicketDto CallTicket(int counterId, string sttId)
        {
            var ticket = _context.Tickets
                .Include(t => t.Service)
                .FirstOrDefault(t => t.SttId == sttId)
                ?? throw new KeyNotFoundException("Không tìm thấy số thứ tự");

            if (ticket.Stage != StageWaiting)
                throw new InvalidOperationException("Số này không ở trạng thái chờ");

            ticket.CounterId = counterId;
            ticket.Stage = StageProcessing;
            _context.SaveChanges();

            return ToDto(ticket);
        }

        // Gọi N số kế tiếp trong hàng chờ, theo thứ tự cấp số
        public List<QueueTicketDto> CallNext(int counterId, int count)
        {
            var tickets = _context.Tickets
                .Include(t => t.Service)
                .Where(t => t.Stage == StageWaiting)
                .OrderBy(t => t.IssuanceTime)
                .Take(count)
                .ToList();

            foreach (var t in tickets)
            {
                t.CounterId = counterId;
                t.Stage = StageProcessing;
            }

            _context.SaveChanges();

            return tickets.Select(ToDto).ToList();
        }

        private static QueueTicketDto ToDto(Tickets t) => new()
        {
            id = t.SttId,
            number = t.TicketNumber,
            issuedAt = t.IssuanceTime.ToString("yyyy-MM-dd HH:mm:ss"),
            serviceName = t.Service?.ServiceName,
            status = t.Stage,
        };
    }
}
