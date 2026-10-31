using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Services
{
    public class CounterTicketSvc : ICounterTicketSvc
    {
        private readonly MyDbContext _context;
        private readonly IScheduleSvc _scheduleSvc;
        private readonly IConfiguration _configuration;

        public CounterTicketSvc(MyDbContext context, IScheduleSvc scheduleSvc, IConfiguration configuration)
        {
            _context = context;
            _scheduleSvc = scheduleSvc;
            _configuration = configuration;
        }

        // OPR-02: GỌI 1 SỐ KẾ TIẾP (Ưu tiên isPriority, né TransferPending, check giờ)
        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> CallNextTicketAsync(int counterId, int userId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null || counter.status == CounterStatus.Closed)
                return (false, "Quầy đang ở trạng thái đóng (Closed), không thể gọi số.", null);

            // Bốc vé: Waiting, né TransferPending, ưu tiên vé isPriority lên trước
            var ticket = await _context.stickers
                .Where(s => s.counterId == counterId
                         && s.status == StickerStatus.Waiting
                         && s.stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.isPriority)
                .ThenBy(s => s.issuanceTime)
                .FirstOrDefaultAsync();

            if (ticket == null)
                return (false, "Hiện không còn số thứ tự nào đang chờ tại quầy này.", null);

            bool inWorkingHours = await _scheduleSvc.IsWithinOperatingHoursAsync(ticket.serviceId);
            if (!inWorkingHours)
                return (false, "Dịch vụ của vé này hiện đã ngoài khung giờ tiếp nhận phục vụ.", null);

            ticket.status = StickerStatus.Processing;
            ticket.stage = StickerStage.Processing;
            ticket.userId = userId;
            ticket.useTime = DateTimeOffset.UtcNow;

            counter.status = CounterStatus.Serving;
            await _context.SaveChangesAsync();

            return (true, $"Đang gọi số phiếu {ticket.sttId}.", MapToDto(ticket, counter.counterName, null));
        }

        // OPR-03 & OPR-08: GỌI NHIỀU SỐ CÙNG LÚC (Lấy batch từ cấu hình, mặc định 6)

        public async Task<(bool Success, string Message, List<TicketOperationResponseDto> Data)> CallMultipleTicketsAsync(int counterId, int userId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null || counter.status == CounterStatus.Closed)
                return (false, "Quầy đang đóng, không thể gọi số.", new List<TicketOperationResponseDto>());

            int batchSize = _configuration.GetValue<int>("CounterOpSettings:MaxCallBatch", 6);

            var tickets = await _context.stickers
                .Where(s => s.counterId == counterId
                         && s.status == StickerStatus.Waiting
                         && s.stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.isPriority)
                .ThenBy(s => s.issuanceTime)
                .Take(batchSize)
                .ToListAsync();

            if (!tickets.Any())
                return (false, "Không có vé nào đang chờ tiếp nhận.", new List<TicketOperationResponseDto>());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var t in tickets)
                {
                    t.status = StickerStatus.Processing;
                    t.stage = StickerStage.Processing;
                    t.userId = userId;
                    t.useTime = DateTimeOffset.UtcNow;
                }

                counter.status = CounterStatus.Serving;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Đã gọi thành công {tickets.Count} số tiếp theo.", tickets.Select(t => MapToDto(t, counter.counterName, null)).ToList());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống khi gọi danh sách vé: {ex.Message}", new List<TicketOperationResponseDto>());
            }
        }

        // OPR-04: HOÀN THÀNH LƯỢT PHỤC VỤ (Chặn vé TransferPending)

        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> CompleteTicketAsync(int counterId, string sttId, int userId)
        {
            var ticket = await _context.stickers.FirstOrDefaultAsync(s => s.sttId == sttId && s.counterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            // Chặn hoàn thành nếu vé đang trong tiến trình chuyển giao
            if (ticket.stage == StickerStage.TransferPending)
                return (false, "Vé đang trong tiến trình chờ chuyển quầy (TransferPending), không thể hoàn thành!", null);

            ticket.status = StickerStatus.Completed;
            ticket.stage = StickerStage.Completed;
            ticket.userId = userId;
            ticket.useTime = DateTimeOffset.UtcNow;

            var hasOtherProcessing = await _context.stickers.AnyAsync(s => s.counterId == counterId && s.status == StickerStatus.Processing && s.sttId != sttId);
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter != null && !hasOtherProcessing)
            {
                counter.status = CounterStatus.Idle;
            }

            await _context.SaveChangesAsync();
            return (true, $"Đã hoàn thành phục vụ phiếu {ticket.sttId}.", MapToDto(ticket, counter?.counterName, null));
        }


        // OPR-05: BỎ QUA / KHÁCH VẮNG MẶT (Chặn vé TransferPending)

        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> SkipTicketAsync(int counterId, string sttId, int userId, SkipTicketRequestDto? dto)
        {
            var ticket = await _context.stickers.FirstOrDefaultAsync(s => s.sttId == sttId && s.counterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            // Chặn bỏ qua nếu vé đang chờ phản hồi chuyển giao
            if (ticket.stage == StickerStage.TransferPending)
                return (false, "Vé đang chờ quầy khác phản hồi chuyển giao, không thể đánh dấu vắng mặt!", null);

            ticket.status = StickerStatus.Absent;
            ticket.stage = StickerStage.Absent;
            ticket.userId = userId;
            ticket.useTime = DateTimeOffset.UtcNow;

            var hasOtherProcessing = await _context.stickers.AnyAsync(s => s.counterId == counterId && s.status == StickerStatus.Processing && s.sttId != sttId);
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter != null && !hasOtherProcessing)
            {
                counter.status = CounterStatus.Idle;
            }

            await _context.SaveChangesAsync();
            return (true, $"Đã ghi nhận khách vắng mặt cho phiếu {ticket.sttId}.", MapToDto(ticket, counter?.counterName, null));
        }


        // OPR-06: GỌI LẠI VÉ (Chỉ gọi lại vé đang Processing hoặc vừa Absent)

        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> RecallTicketAsync(int counterId, string sttId, int userId)
        {
            var ticket = await _context.stickers.FirstOrDefaultAsync(s => s.sttId == sttId && s.counterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            if (ticket.stage == StickerStage.TransferPending)
                return (false, "Không thể gọi lại vé đang trong trạng thái chuyển quầy.", null);

            if (ticket.status != StickerStatus.Processing && ticket.status != StickerStatus.Absent)
                return (false, "Chỉ được phép gọi lại vé đang xử lý (Processing) hoặc vé vừa bị bỏ qua (Absent).", null);

            ticket.status = StickerStatus.Processing;
            ticket.stage = StickerStage.Processing;
            ticket.userId = userId;
            ticket.useTime = DateTimeOffset.UtcNow;

            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter != null) counter.status = CounterStatus.Serving;

            await _context.SaveChangesAsync();
            return (true, $"Đang gọi lại số phiếu {ticket.sttId}.", MapToDto(ticket, counter?.counterName, null));
        }

        // Helper Map Entity sang DTO
        private static TicketOperationResponseDto MapToDto(Sticker ticket, string? counterName, string? staffName)
        {
            return new TicketOperationResponseDto
            {
                sttId = ticket.sttId,
                serviceId = ticket.serviceId,
                counterId = ticket.counterId,
                counterName = counterName ?? string.Empty,
                userId = ticket.userId,
                staffName = staffName,
                status = ticket.status,
                stage = ticket.stage,
                isPriority = ticket.isPriority,
                issuanceTime = ticket.issuanceTime,
                useTime = ticket.useTime
            };
        }
    }
}