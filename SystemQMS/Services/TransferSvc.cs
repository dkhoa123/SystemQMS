using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.TransfersDto;

namespace SystemQMS.Services
{
    public class TransferSvc : ITransferSvc
    {
        private readonly MyDbContext _context;
        private readonly ILogger<TransferSvc> _logger;

        public TransferSvc(MyDbContext context, ILogger<TransferSvc> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static TransferResponse ToResponse(CounterChange change) => new()
        {
            id = change.id,
            sttId = change.sttId,
            counterIdSender = change.counterIdSender,
            counterIdReceive = change.counterIdReceive,
            status = change.status,
            createTime = change.createTime
        };

        // OPR-07: tạo yêu cầu chuyển phiếu.
        // CounterChange Pending + stage TransferPending trong 1 transaction
        public async Task<List<TransferResponse>> CreateTransferAsync(
            int counterId, TransferRequest request, CancellationToken ct = default)
        {
            var sender = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId, ct);
            if (sender == null)
                throw new KeyNotFoundException("Quầy gửi không tồn tại");
            if (!sender.isActive || sender.status == CounterStatus.Closed)
                throw new ArgumentException("Quầy gửi đang đóng, không thể chuyển phiếu");

            if (request.toCounterId == counterId)
                throw new ArgumentException("Quầy nhận phải khác quầy gửi");

            var receiver = await _context.counters.FirstOrDefaultAsync(c => c.id == request.toCounterId, ct);
            if (receiver == null)
                throw new ArgumentException("Quầy nhận không tồn tại");
            if (!receiver.isActive || receiver.status == CounterStatus.Closed
                || receiver.status == CounterStatus.PausedAccepting)
                throw new ArgumentException("Quầy nhận không hoạt động");

            var sttIds = (request.sttIds ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();
            if (sttIds.Count == 0)
                throw new ArgumentException("Chưa chọn phiếu để chuyển");

            var stickers = await _context.stickers
                .Where(s => sttIds.Contains(s.sttId) && s.counterId == counterId)
                .ToListAsync(ct);
            if (stickers.Count != sttIds.Count)
                throw new ArgumentException("Có phiếu không tồn tại hoặc không thuộc quầy gửi");

            foreach (var sticker in stickers)
            {
                // SRS: chuyển được cả phiếu đang chờ lẫn phiếu đang xử lý
                if (sticker.status != StickerStatus.Waiting
                    && sticker.status != StickerStatus.Processing)
                    throw new ArgumentException($"Phiếu {sticker.sttId} không ở trạng thái chờ hoặc đang xử lý");
                if (sticker.stage == StickerStage.TransferPending)
                    throw new ArgumentException($"Phiếu {sticker.sttId} đang chờ quầy khác phản hồi");
            }

            var now = DateTimeOffset.UtcNow;
            var changes = new List<CounterChange>();

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var sticker in stickers)
                {
                    var change = new CounterChange
                    {
                        sttId = sticker.sttId,
                        counterIdSender = counterId,
                        counterIdReceive = request.toCounterId,
                        status = CounterChangeStatus.Pending,
                        createTime = now
                    };
                    _context.counterChanges.Add(change);
                    changes.Add(change);

                    sticker.stage = StickerStage.TransferPending;
                    if (request.isPriority == true)
                        sticker.isPriority = true;
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            _logger.LogInformation(
                "Tạo yêu cầu chuyển quầy: From={From}, To={To}, Count={Count}",
                counterId, request.toCounterId, changes.Count);

            return changes.Select(ToResponse).ToList();
        }

        // OPR-07: danh sách yêu cầu đang chờ quầy này tiếp nhận
        public async Task<List<TransferResponse>> GetPendingTransfersAsync(
            int counterId, CancellationToken ct = default)
        {
            if (!await _context.counters.AnyAsync(c => c.id == counterId, ct))
                throw new KeyNotFoundException("Quầy không tồn tại");

            var pending = await _context.counterChanges.AsNoTracking()
                .Where(cc => cc.counterIdReceive == counterId
                             && cc.status == CounterChangeStatus.Pending)
                .OrderByDescending(cc => cc.createTime)
                .ToListAsync(ct);

            return pending.Select(ToResponse).ToList();
        }

        // OPR-07: quầy nhận bấm đồng ý/từ chối
        public async Task<TransferResponse> ActionAsync(
            int transferId, string action, CancellationToken ct = default)
        {
            var change = await _context.counterChanges.FirstOrDefaultAsync(c => c.id == transferId, ct);
            if (change == null)
                throw new KeyNotFoundException("Yêu cầu chuyển quầy không tồn tại");
            if (change.status != CounterChangeStatus.Pending)
                throw new ArgumentException("Yêu cầu chuyển quầy đã được xử lý");

            if (string.Equals(action, "Accept", StringComparison.OrdinalIgnoreCase))
            {
                await ApplyAcceptAsync(_context, change, isAuto: false, ct);
                _logger.LogInformation("Chấp nhận chuyển quầy: TransferId={TransferId}, SttId={SttId}",
                    change.id, change.sttId);
            }
            else if (string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase))
            {
                await ApplyRejectAsync(_context, change, ct);
                _logger.LogInformation("Từ chối chuyển quầy: TransferId={TransferId}, SttId={SttId}",
                    change.id, change.sttId);
            }
            else
            {
                throw new ArgumentException("Hành động không hợp lệ, chỉ nhận Accept hoặc Reject");
            }

            return ToResponse(change);
        }

        // Hàm lõi dùng chung cho cả nút bấm Accept lẫn job timeout (AutoAccepted)
        public static async Task ApplyAcceptAsync(
            MyDbContext context, CounterChange change, bool isAuto, CancellationToken ct = default)
        {
            var sticker = await context.stickers.FirstOrDefaultAsync(s => s.sttId == change.sttId, ct);
            if (sticker == null)
                throw new KeyNotFoundException("Phiếu không tồn tại");
           
            bool wasProcessing = sticker.status == StickerStatus.Processing;

            sticker.counterId = change.counterIdReceive;
            sticker.stage = StickerStage.Accepted;
            sticker.status = StickerStatus.Waiting;

           
            if (!sticker.isPriority)
                sticker.issuanceTime = DateTimeOffset.UtcNow;

            change.status = isAuto ? CounterChangeStatus.AutoAccepted : CounterChangeStatus.Accepted;

            // Quầy gửi mất phiếu Processing cuối cùng thì hết trạng thái đang phục vụ
            if (wasProcessing)
            {
                bool senderStillProcessing = await context.stickers.AnyAsync(
                    s => s.counterId == change.counterIdSender
                         && s.status == StickerStatus.Processing
                         && s.sttId != sticker.sttId, ct);
                if (!senderStillProcessing)
                {
                    var sender = await context.counters
                        .FirstOrDefaultAsync(c => c.id == change.counterIdSender, ct);
                    if (sender != null && sender.status == CounterStatus.Serving)
                        sender.status = CounterStatus.Idle;
                }
            }

            await context.SaveChangesAsync(ct);
        }

        // Từ chối: phiếu ở nguyên quầy gửi, quay lại hàng chờ bình thường
        public static async Task ApplyRejectAsync(
            MyDbContext context, CounterChange change, CancellationToken ct = default)
        {
            var sticker = await context.stickers.FirstOrDefaultAsync(s => s.sttId == change.sttId, ct);
            if (sticker == null)
                throw new KeyNotFoundException("Phiếu không tồn tại");

            sticker.stage = StickerStage.Waiting;
            change.status = CounterChangeStatus.Rejected;
            await context.SaveChangesAsync(ct);
        }
    }
}
