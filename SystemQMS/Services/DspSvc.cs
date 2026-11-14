using Microsoft.EntityFrameworkCore;
using SystemQMS.ViewModels.DspDto;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.Constants;

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

            var counters = await _context.Counters
                .AsNoTracking()
                .Where(c => c.IsActive
                            && c.UnitId == unitId
                            && c.Status != CounterStatus.PausedAccepting)
                .Select(c => new
                {
                    c.Id,
                    Name = c.CounterName ?? c.CounterId
                })
                .ToListAsync();

            var counterMap = counters.ToDictionary(c => c.Id, c => c.Name);
            var counterIds = counterMap.Keys.ToList();

            var calledTickets = await _context.Tickets
                .AsNoTracking()
                .Where(s => counterIds.Contains(s.CounterId) && s.Status == StickerStatus.Processing)
                .Select(s => new
                {
                    s.CounterId,
                    s.SttId,
                    CalledAt = s.UseTime == default ? s.IssuanceTime : s.UseTime
                })
                .OrderByDescending(s => s.CalledAt)
                .ThenBy(s => s.CounterId)
                .ToListAsync();

            var pageSize = showWaitlist ? 5 : 10;
            var rows = new List<DisplayBoardRowDto>();

            if (showWaitlist)
            {
                var waitingByCounter = await _context.Tickets
                    .AsNoTracking()
                    .Where(s => counterIds.Contains(s.CounterId)
                                && s.Status == StickerStatus.Waiting
                                && s.Stage != StickerStage.TransferPending)
                    .OrderByDescending(s => s.IsPriority)
                    .ThenBy(s => s.IssuanceTime)
                    .Select(s => new
                    {

                        s.CounterId,
                        s.SttId
                    })
                    .ToListAsync();

                var waitingLookup = waitingByCounter
                    .GroupBy(x => x.CounterId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                var waitingIndexByCounter = waitingLookup.Keys.ToDictionary(k => k, _ => 0);

                foreach (var called in calledTickets)
                {
                    waitingLookup.TryGetValue(called.CounterId, out var waitingList);

                    string? waitingSttId = null;
                    int? waitingTicketNumber = null;

                    if (waitingList != null
                        && waitingIndexByCounter.TryGetValue(called.CounterId, out var waitingIndex)
                        && waitingIndex < waitingList.Count)
                    {
                        waitingSttId = waitingList[waitingIndex].SttId;
                        waitingTicketNumber = TryExtractTicketNumber(waitingSttId);
                        waitingIndexByCounter[called.CounterId] = waitingIndex + 1;
                    }

                    rows.Add(new DisplayBoardRowDto
                    {
                        CounterId = called.CounterId,
                        CounterName = counterMap.TryGetValue(called.CounterId, out var name) ? name : string.Empty,
                        CalledSttId = called.SttId,
                        CalledTicketNumber = TryExtractTicketNumber(called.SttId),
                        WaitingSttId = waitingSttId,
                        WaitingTicketNumber = waitingTicketNumber
                    });
                }
            }
            else
            {
                rows = calledTickets.Select(called => new DisplayBoardRowDto
                {
                    CounterId = called.CounterId,
                    CounterName = counterMap.TryGetValue(called.CounterId, out var name) ? name : string.Empty,
                    CalledSttId = called.SttId,
                    CalledTicketNumber = TryExtractTicketNumber(called.SttId)
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
            var counters = await _context.Counters
                .AsNoTracking()
                .Where(c => c.IsActive
                            && c.UnitId == unitId
                            && c.Status != CounterStatus.PausedAccepting)
                .Select(c => new
                {
                    c.Id,
                    Name = c.CounterName ?? c.CounterId,
                    c.Status
                })
                .ToListAsync();

            var counterIds = counters.Select(c => c.Id).ToList();
            var servingCounterIds = await _context.Tickets
                .AsNoTracking()
                .Where(s => counterIds.Contains(s.CounterId) && s.Status == StickerStatus.Processing)
                .Select(s => s.CounterId)
                .Distinct()
                .ToListAsync();

            var servingSet = servingCounterIds.ToHashSet();

            return new DisplayCounterStatusResponseDto
            {
                CurrentTime = DateTime.Now,
                Counters = counters
                    .Select(c =>
                    {
                        var isServing = servingSet.Contains(c.Id);
                        return new DisplayCounterStatusDto
                        {
                            CounterId = c.Id,
                            CounterName = c.Name,
                            CounterStatus = c.Status,
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
            var counter = await _context.Counters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == counterId);

            if (counter == null) return null;

            var response = new CounterScreenStateResponseDto
            {
                CurrentTime = DateTime.Now,
                CounterId = counter.Id,
                CounterName = counter.CounterName ?? counter.CounterId,
                ShowWaitlist = showWaitlist
            };

            if (!counter.IsActive || counter.Status == CounterStatus.Closed)
            {
                response.ScreenState = "Closed";
                response.ScreenMessage = "QUẦY ĐÃ ĐÓNG";
                response.IsGrayScreen = true;
                return response;
            }

            var called = await _context.Tickets
                .AsNoTracking()
                .Where(s => s.CounterId == counterId && s.Status == StickerStatus.Processing)
                .OrderByDescending(s => s.UseTime)
                .ThenByDescending(s => s.IssuanceTime)
                .Select(s => new CounterCalledTicketDto
                {
                    SttId = s.SttId,
                    TicketNumber = TryExtractTicketNumber(s.SttId)
                })
                .FirstOrDefaultAsync();

            var waiting = showWaitlist
                ? await _context.Tickets
                    .AsNoTracking()
                    .Where(s => s.CounterId == counterId
                                && s.Status == StickerStatus.Waiting
                                && s.Stage != StickerStage.TransferPending)
                    .OrderByDescending(s => s.IsPriority)
                    .ThenBy(s => s.IssuanceTime)
                    .Take(9)
                    .Select(s => new CounterWaitingTicketDto
                    {
                        SttId = s.SttId,
                        TicketNumber = TryExtractTicketNumber(s.SttId)
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

            var counter = await _context.Counters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == counterId);

            if (counter == null) return null;

            var callingTickets = await _context.Tickets
                .AsNoTracking()
                .Where(s => s.CounterId == counterId && s.Status == StickerStatus.Processing)
                .OrderByDescending(s => s.UseTime)
                .ThenByDescending(s => s.IssuanceTime)
                .Select(s => new CounterCalledTicketDto
                {
                    SttId = s.SttId,
                    TicketNumber = TryExtractTicketNumber(s.SttId)
                })
                .ToListAsync();

            var waitingTickets = await _context.Tickets
                .AsNoTracking()
                .Where(s => s.CounterId == counterId
                            && s.Status == StickerStatus.Waiting
                            && s.Stage != StickerStage.TransferPending)
                .OrderByDescending(s => s.IsPriority)
                .ThenBy(s => s.IssuanceTime)
                .Take(waitingLimit)
                .Select(s => new CounterWaitingTicketDto
                {
                    SttId = s.SttId,
                    TicketNumber = TryExtractTicketNumber(s.SttId)
                })
                .ToListAsync();

            return new CounterMultiCallDisplayResponseDto
            {
                CurrentTime = DateTime.Now,
                CounterId = counter.Id,
                CounterName = counter.CounterName ?? counter.CounterId,
                CallingTickets = callingTickets,
                WaitingTickets = waitingTickets
            };
        }

        public async Task<DisplayMaintenanceResponseDto> ReportDisplayErrorAndSwitchMaintenanceAsync(string deviceId, DisplayErrorRequestDto error)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device != null)
            {
                device.StatusConnect = "MAINTENANCE";
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