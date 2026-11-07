using Microsoft.EntityFrameworkCore;
using SystemQMS.ViewModels.DspDto;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class DspSvc : IDspSvc
    {
        private const int AutoSwitchSeconds = 15;

        private readonly MyDbContext _context;

        public DspSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<DisplayBoardPageResponseDto> GetDisplayBoardPageAsync(int page, bool showWaitlist, int unitId)
        {
            if (page < 1) page = 1;

            var counters = await _context.counters
                .AsNoTracking()
                .Where(c => c.isActive && c.unitId == unitId)
                .Select(c => new
                {
                    c.id,
                    Name = c.counterName ?? c.counterId
                })
                .ToListAsync();

            var counterMap = counters.ToDictionary(c => c.id, c => c.Name);
            var counterIds = counterMap.Keys.ToList();

            var calledTickets = await _context.stickers
                .AsNoTracking()
                .Where(s => counterIds.Contains(s.counterId) && s.status == StickerStatus.Processing)
                .Select(s => new
                {
                    s.counterId,
                    s.sttId,
                    CalledAt = s.useTime == default ? s.issuanceTime : s.useTime
                })
                .OrderByDescending(s => s.CalledAt)
                .ThenBy(s => s.counterId)
                .ToListAsync();

            var pageSize = showWaitlist ? 5 : 10;
            var rows = new List<DisplayBoardRowDto>();

            if (showWaitlist)
            {
                var waitingByCounter = await _context.stickers
                    .AsNoTracking()
                    .Where(s => counterIds.Contains(s.counterId)
                                && s.status == StickerStatus.Waiting
                                && s.stage != StickerStage.TransferPending)
                    .OrderByDescending(s => s.isPriority)
                    .ThenBy(s => s.issuanceTime)
                    .Select(s => new
                    {
                        s.counterId,
                        s.sttId
                    })
                    .ToListAsync();

                var waitingLookup = waitingByCounter
                    .GroupBy(x => x.counterId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                var waitingIndexByCounter = waitingLookup.Keys.ToDictionary(k => k, _ => 0);

                foreach (var called in calledTickets)
                {
                    waitingLookup.TryGetValue(called.counterId, out var waitingList);

                    string? waitingSttId = null;
                    int? waitingTicketNumber = null;

                    if (waitingList != null
                        && waitingIndexByCounter.TryGetValue(called.counterId, out var waitingIndex)
                        && waitingIndex < waitingList.Count)
                    {
                        waitingSttId = waitingList[waitingIndex].sttId;
                        waitingTicketNumber = TryExtractTicketNumber(waitingSttId);
                        waitingIndexByCounter[called.counterId] = waitingIndex + 1;
                    }

                    rows.Add(new DisplayBoardRowDto
                    {
                        CounterId = called.counterId,
                        CounterName = counterMap.TryGetValue(called.counterId, out var name) ? name : string.Empty,
                        CalledSttId = called.sttId,
                        CalledTicketNumber = TryExtractTicketNumber(called.sttId),
                        WaitingSttId = waitingSttId,
                        WaitingTicketNumber = waitingTicketNumber
                    });
                }
            }
            else
            {
                rows = calledTickets.Select(called => new DisplayBoardRowDto
                {
                    CounterId = called.counterId,
                    CounterName = counterMap.TryGetValue(called.counterId, out var name) ? name : string.Empty,
                    CalledSttId = called.sttId,
                    CalledTicketNumber = TryExtractTicketNumber(called.sttId)
                }).ToList();
            }

            var totalRows = rows.Count;
            var totalPages = totalRows == 0 ? 1 : (int)Math.Ceiling(totalRows / (double)pageSize);
            if (page > totalPages) page = totalPages;

            var pagedRows = rows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new DisplayBoardPageResponseDto
            {
                CurrentTime = DateTime.Now,
                ShowWaitlist = showWaitlist,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRows = totalRows,
                AutoSwitchSeconds = AutoSwitchSeconds,
                NextPage = page >= totalPages ? 1 : page + 1,
                Rows = pagedRows
            };
        }

        private static int? TryExtractTicketNumber(string? sttId)
        {
            if (string.IsNullOrWhiteSpace(sttId)) return null;

            var digits = new string(sttId.Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits)) return null;

            return int.TryParse(digits, out var number) ? number : null;
        }
    }
}