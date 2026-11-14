using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SystemQMS.Constants;
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
        private readonly IAudioMappingSvc _audioMappingSvc;

        public CounterTicketSvc(MyDbContext context, IScheduleSvc scheduleSvc, IConfiguration configuration, IAudioMappingSvc audioMappingSvc)
        {
            _context = context;
            _scheduleSvc = scheduleSvc;
            _configuration = configuration;
            _audioMappingSvc = audioMappingSvc;
        }

        // OPR-02: GỌI 1 SỐ KẾ TIẾP (Ưu tiên isPriority, né TransferPending, check giờ)
        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> CallNextTicketAsync(int counterId, int userId)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null || counter.Status == CounterStatus.Closed)
                return (false, "Quầy đang ở trạng thái đóng (Closed), không thể gọi số.", null);

            // Bốc vé: Waiting, né TransferPending, ưu tiên vé isPriority lên trước
            var ticket = await _context.Tickets
                .Where(s => s.CounterId == counterId
                         && s.Status == StickerStatus.Waiting
                         && s.Stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.IsPriority)
                .ThenBy(s => s.IssuanceTime)
                .FirstOrDefaultAsync();

            if (ticket == null)
                return (false, "Hiện không còn số thứ tự nào đang chờ tại quầy này.", null);

            bool inWorkingHours = await _scheduleSvc.IsWithinOperatingHoursAsync(ticket.ServiceId);
            if (!inWorkingHours)
                return (false, "Dịch vụ của vé này hiện đã ngoài khung giờ tiếp nhận phục vụ.", null);

            ticket.Status = StickerStatus.Processing;
            ticket.Stage = StickerStage.Processing;
            ticket.UserId = userId;
            ticket.UseTime = DateTimeOffset.UtcNow;

            counter.Status = CounterStatus.Serving;
            await _context.SaveChangesAsync();

            var (templateId, audioSequence) = await BuildAudioSequenceAsync(counter, ticket.SttId);
            var response = MapToDto(ticket, counter.CounterName, null);
            response.audioTemplateId = templateId;
            response.audioSequence = audioSequence;

            return (true, $"Đang gọi số phiếu {ticket.SttId}.", response);
        }

        // OPR-03 & OPR-08: GỌI NHIỀU SỐ CÙNG LÚC (Lấy batch từ cấu hình, mặc định 6)

        public async Task<(bool Success, string Message, List<TicketOperationResponseDto> Data)> CallMultipleTicketsAsync(int counterId, int userId)
        {
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter == null || counter.Status == CounterStatus.Closed)
                return (false, "Quầy đang đóng, không thể gọi số.", new List<TicketOperationResponseDto>());

            int batchSize = _configuration.GetValue<int>("CounterOpSettings:MaxCallBatch", 6);

            var tickets = await _context.Tickets
                .Where(s => s.CounterId == counterId
                         && s.Status == StickerStatus.Waiting
                         && s.Stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.IsPriority)
                .ThenBy(s => s.IssuanceTime)
                .Take(batchSize)
                .ToListAsync();

            if (!tickets.Any())
                return (false, "Không có vé nào đang chờ tiếp nhận.", new List<TicketOperationResponseDto>());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var t in tickets)
                {
                    t.Status = StickerStatus.Processing;
                    t.Stage = StickerStage.Processing;
                    t.UserId = userId;
                    t.UseTime = DateTimeOffset.UtcNow;
                }

                counter.Status = CounterStatus.Serving;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = new List<TicketOperationResponseDto>();
                foreach (var t in tickets)
                {
                    var mapped = MapToDto(t, counter.CounterName, null);
                    var (templateId, audioSequence) = await BuildAudioSequenceAsync(counter, t.SttId);
                    mapped.audioTemplateId = templateId;
                    mapped.audioSequence = audioSequence;
                    response.Add(mapped);
                }

                return (true, $"Đã gọi thành công {tickets.Count} số tiếp theo.", response);
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
            var ticket = await _context.Tickets.FirstOrDefaultAsync(s => s.SttId == sttId && s.CounterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            // Chặn hoàn thành nếu vé đang trong tiến trình chuyển giao
            if (ticket.Stage == StickerStage.TransferPending)
                return (false, "Vé đang trong tiến trình chờ chuyển quầy (TransferPending), không thể hoàn thành!", null);

            ticket.Status = StickerStatus.Completed;
            ticket.Stage = StickerStage.Completed;
            ticket.UserId = userId;
            ticket.UseTime = DateTimeOffset.UtcNow;

            var hasOtherProcessing = await _context.Tickets.AnyAsync(s => s.CounterId == counterId && s.Status == StickerStatus.Processing && s.SttId != sttId);
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter != null && !hasOtherProcessing)
            {
                counter.Status = CounterStatus.Idle;
            }

            await _context.SaveChangesAsync();
            return (true, $"Đã hoàn thành phục vụ phiếu {ticket.SttId}.", MapToDto(ticket, counter?.CounterName, null));
        }


        // OPR-05: BỎ QUA / KHÁCH VẮNG MẶT (Chặn vé TransferPending)

        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> SkipTicketAsync(int counterId, string sttId, int userId, SkipTicketRequestDto? dto)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(s => s.SttId == sttId && s.CounterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            // Chặn bỏ qua nếu vé đang chờ phản hồi chuyển giao
            if (ticket.Stage == StickerStage.TransferPending)
                return (false, "Vé đang chờ quầy khác phản hồi chuyển giao, không thể đánh dấu vắng mặt!", null);

            ticket.Status = StickerStatus.Absent;
            ticket.Stage = StickerStage.Absent;
            ticket.UserId = userId;
            ticket.UseTime = DateTimeOffset.UtcNow;

            var hasOtherProcessing = await _context.Tickets.AnyAsync(s => s.CounterId == counterId && s.Status == StickerStatus.Processing && s.SttId != sttId);
            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter != null && !hasOtherProcessing)
            {
                counter.Status = CounterStatus.Idle;
            }

            await _context.SaveChangesAsync();
            return (true, $"Đã ghi nhận khách vắng mặt cho phiếu {ticket.SttId}.", MapToDto(ticket, counter?.CounterName, null));
        }


        // OPR-06: GỌI LẠI VÉ (Chỉ gọi lại vé đang Processing hoặc vừa Absent)

        public async Task<(bool Success, string Message, TicketOperationResponseDto? Data)> RecallTicketAsync(int counterId, string sttId, int userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(s => s.SttId == sttId && s.CounterId == counterId);
            if (ticket == null) return (false, "Không tìm thấy vé tại quầy này.", null);

            if (ticket.Stage == StickerStage.TransferPending)
                return (false, "Không thể gọi lại vé đang trong trạng thái chuyển quầy.", null);

            if (ticket.Status != StickerStatus.Processing && ticket.Status != StickerStatus.Absent)
                return (false, "Chỉ được phép gọi lại vé đang xử lý (Processing) hoặc vé vừa bị bỏ qua (Absent).", null);

            ticket.Status = StickerStatus.Processing;
            ticket.Stage = StickerStage.Processing;
            ticket.UserId = userId;
            ticket.UseTime = DateTimeOffset.UtcNow;

            var counter = await _context.Counters.FirstOrDefaultAsync(c => c.Id == counterId);
            if (counter != null) counter.Status = CounterStatus.Serving;

            await _context.SaveChangesAsync();

            var response = MapToDto(ticket, counter?.CounterName, null);
            if (counter != null)
            {
                var (templateId, audioSequence) = await BuildAudioSequenceAsync(counter, ticket.SttId);
                response.audioTemplateId = templateId;
                response.audioSequence = audioSequence;
            }

            return (true, $"Đang gọi lại số phiếu {ticket.SttId}.", response);
        }

        private async Task<(int templateId, List<string> audioSequence)> BuildAudioSequenceAsync(Counter counter, string sttId)
        {
            var configuredTemplateId = await _context.UnitConfigs
                .AsNoTracking()
                .Where(x => x.UnitId == counter.UnitId)
                .Select(x => x.ActiveAudioTemplateId)
                .FirstOrDefaultAsync();

            var templateId = configuredTemplateId ?? 1;
            var sequence = _audioMappingSvc.BuildAudioSequence(sttId, counter.CounterName ?? counter.CounterId, templateId);
            return (templateId, sequence);
        }

        // Helper Map Entity sang DTO
        private static TicketOperationResponseDto MapToDto(Tickets ticket, string? counterName, string? staffName)
        {
            return new TicketOperationResponseDto
            {
                sttId = ticket.SttId,
                serviceId = ticket.ServiceId,
                counterId = ticket.CounterId,
                counterName = counterName ?? string.Empty,
                userId = ticket.UserId,
                staffName = staffName,
                status = ticket.Status,
                stage = ticket.Stage,
                isPriority = ticket.IsPriority,
                issuanceTime = ticket.IssuanceTime,
                useTime = ticket.UseTime
            };
        }
    }
}