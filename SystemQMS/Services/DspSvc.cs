using Microsoft.EntityFrameworkCore;
using SystemQMS.ViewModels.DspDto;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class DspSvc : IDspSvc
    {
        private readonly MyDbContext _context;
        private readonly ILogger<DspSvc> _logger;

        public DspSvc(MyDbContext context, ILogger<DspSvc> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MainDisplayResponseDto> GetMainDisplayStateAsync()
        {

            var counters = await _context.counters
                .Where(c => c.isActive && c.status != "STOP_ACCEPTING")
                .Select(c => new MainDisplayCounterDto
                {
                    CounterId = c.id,
                    CounterName = c.counterName ?? c.counterId,
                    HasActiveTicket = _context.Tickets.Any(t => t.CounterId == c.id && t.status == "CALLING"),
                    CallingTickets = _context.Tickets
                        .Where(t => t.CounterId == c.id && t.status == "CALLING")
                        .Select(t => t.ticketNumber)
                        .ToList(),
                    WaitingTickets = _context.Tickets
                        .Where(t => t.CounterId == c.id && t.status == "WAITING")
                        .OrderBy(t => t.createdAt)
                        .Take(5) 
                        .Select(t => t.ticketNumber)
                        .ToList()
                })
                .ToListAsync();

            return new MainDisplayResponseDto
            {
                CurrentTime = DateTime.UtcNow,
                ShowWaitlist = true,
                Counters = counters
            };
        }

        public async Task<CounterDisplayResponseDto?> GetCounterDisplayStateAsync(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(c => c.id == counterId);
            if (counter == null) return null;

            var callingTickets = await _context.Tickets
                .Where(t => t.CounterId == counterId && t.status == "CALLING")
                .Select(t => new TicketDisplayDto
                {
                    TicketNumber = t.ticketNumber,
                    IsProcessed = false
                })
                .ToListAsync();

            var waitingTickets = await _context.Tickets
                .Where(t => t.CounterId == counterId && t.status == "WAITING")
                .OrderBy(t => t.createdAt)
                .Take(9)
                .Select(t => t.ticketNumber)
                .ToListAsync();

            return new CounterDisplayResponseDto
            {
                CounterId = counter.id,
                Status = counter.status,
                ShowWaitlist = true,
                CallingTickets = callingTickets,
                WaitingTickets = waitingTickets
            };
        }

        public async Task LogDeviceErrorAsync(string deviceId, ErrorLogRequestDto error)
        {
            var device = await _context.devices.FirstOrDefaultAsync(d => d.deviceId == deviceId);
            if (device != null)
            {
                device.statusConnect = "ERROR";

                await _context.SaveChangesAsync();
            }
        }
    }
}