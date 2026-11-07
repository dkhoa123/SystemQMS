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
                .Where(c => c.isActive
                            && c.unitId == unitId
                            && c.status != CounterStatus.PausedAccepting)
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

        public async Task<DisplayCounterStatusResponseDto> GetDisplayCounterStatusAsync(int unitId)
        {
            var counters = await _context.counters
                .AsNoTracking()
                .Where(c => c.isActive
                            && c.unitId == unitId
                            && c.status != CounterStatus.PausedAccepting)
                .Select(c => new
                {
                    c.id,
                    Name = c.counterName ?? c.counterId,
                    c.status
                })
                .ToListAsync();

            var counterIds = counters.Select(c => c.id).ToList();
            var servingCounterIds = await _context.stickers
                .AsNoTracking()
                .Where(s => counterIds.Contains(s.counterId) && s.status == StickerStatus.Processing)
                .Select(s => s.counterId)
                .Distinct()
                .ToListAsync();

            var servingSet = servingCounterIds.ToHashSet();

            return new DisplayCounterStatusResponseDto
            {
                CurrentTime = DateTime.Now,
                Counters = counters
                    .Select(c =>
                    {
                        var isServing = servingSet.Contains(c.id);
                        return new DisplayCounterStatusDto
                        {
                            CounterId = c.id,
                            CounterName = c.Name,
                            CounterStatus = c.status,
                            IsServing = isServing,
                            IsDimmed = !isServing
                        };
                    })
                    .OrderBy(c => c.CounterId)
                    .ToList()
            };
        }

        public async Task<CounterScreenStateResponseDto?> GetCounterScreenStateAsync(int counterId, bool showWaitlist)
        {
            var counter = await _context.counters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.id == counterId);

            if (counter == null) return null;

            var response = new CounterScreenStateResponseDto
            {
                CurrentTime = DateTime.Now,
                CounterId = counter.id,
                CounterName = counter.counterName ?? counter.counterId,
                ShowWaitlist = showWaitlist
            };

            if (!counter.isActive || counter.status == CounterStatus.Closed)
            {
                response.ScreenState = "Closed";
                response.ScreenMessage = "QUẦY ĐÃ ĐÓNG";
                response.IsGrayScreen = true;
                return response;
            }

            var called = await _context.stickers
                .AsNoTracking()
                .Where(s => s.counterId == counterId && s.status == StickerStatus.Processing)
                .OrderByDescending(s => s.useTime)
                .ThenByDescending(s => s.issuanceTime)
                .Select(s => new CounterCalledTicketDto
                {
                    SttId = s.sttId,
                    TicketNumber = TryExtractTicketNumber(s.sttId)
                })
                .FirstOrDefaultAsync();

            var waiting = showWaitlist
                ? await _context.stickers
                    .AsNoTracking()
                    .Where(s => s.counterId == counterId
                                && s.status == StickerStatus.Waiting
                                && s.stage != StickerStage.TransferPending)
                    .OrderByDescending(s => s.isPriority)
                    .ThenBy(s => s.issuanceTime)
                    .Take(9)
                    .Select(s => new CounterWaitingTicketDto
                    {
                        SttId = s.sttId,
                        TicketNumber = TryExtractTicketNumber(s.sttId)
                    })
                    .ToListAsync()
                : new List<CounterWaitingTicketDto>();

            response.CalledTicket = called;
            response.WaitingTickets = waiting;

            if (called == null && waiting.Count == 0)
            {
                response.ScreenState = "TemporarilyClosed";
                response.ScreenMessage = "TẠM ĐÓNG";
                response.IsGrayScreen = false;
                return response;
            }

            response.ScreenState = "Active";
            response.ScreenMessage = null;
            response.IsGrayScreen = false;
            return response;
        }

        public async Task<CounterMultiCallDisplayResponseDto?> GetCounterMultiCallDisplayAsync(int counterId, int waitingLimit)
        {
            if (waitingLimit < 1) waitingLimit = 1;
            if (waitingLimit > 9) waitingLimit = 9;

            var counter = await _context.counters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.id == counterId);

            if (counter == null) return null;

            var callingTickets = await _context.stickers
                .AsNoTracking()
                .Where(s => s.counterId == counterId && s.status == StickerStatus.Processing)
                .OrderByDescending(s => s.useTime)
                .ThenByDescending(s => s.issuanceTime)
                .Select(s => new CounterCalledTicketDto
                {
                    SttId = s.sttId,
                    TicketNumber = TryExtractTicketNumber(s.sttId)
                })
                .ToListAsync();

            var waitingTickets = await _context.stickers
                .AsNoTracking()
                .Where(s => s.counterId == counterId
                            && s.status == StickerStatus.Waiting
                            && s.stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.isPriority)
                .ThenBy(s => s.issuanceTime)
                .Take(waitingLimit)
                .Select(s => new CounterWaitingTicketDto
                {
                    SttId = s.sttId,
                    TicketNumber = TryExtractTicketNumber(s.sttId)
                })
                .ToListAsync();

            return new CounterMultiCallDisplayResponseDto
            {
                CurrentTime = DateTime.Now,
                CounterId = counter.id,
                CounterName = counter.counterName ?? counter.counterId,
                CallingTickets = callingTickets,
                WaitingTickets = waitingTickets
            };
        }

        public async Task<DisplayMaintenanceResponseDto> ReportDisplayErrorAndSwitchMaintenanceAsync(string deviceId, DisplayErrorRequestDto error)
        {
            var device = await _context.devices.FirstOrDefaultAsync(d => d.deviceId == deviceId);
            if (device != null)
            {
                device.statusConnect = "MAINTENANCE";
                await _context.SaveChangesAsync();
            }

            return new DisplayMaintenanceResponseDto
            {
                DeviceId = deviceId,
                Mode = "MAINTENANCE",
                Message = $"{error.ErrorCode}: {error.Message}",
                UpdatedAt = DateTime.Now
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