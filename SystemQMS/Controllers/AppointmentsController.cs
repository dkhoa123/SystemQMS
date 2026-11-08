
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SystemQMS.Models;
using SystemQMS.ViewModels.Appointment;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public AppointmentsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Appointments/service
        // Khách chưa đăng nhập vẫn xem được dịch vụ
        [HttpGet("service")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.services
                .AsNoTracking()
                .Where(s => s.isActive && s.allowOnlineBooking)
                .Select(s => new
                {
                    s.id,
                    s.serviceId,
                    s.serviceName,
                    s.description,
                    s.image,
                    s.sharingLink,
                    s.averageTime
                })
                .ToListAsync();

            return Ok(new
            {
                message = services.Count > 0
                    ? "Lấy danh sách dịch vụ thành công"
                    : "Không có dịch vụ nào",
                data = services,
                total = services.Count
            });
        }

        // GET: api/Appointments/available-date
        [HttpGet("available-date")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTime(
            [FromQuery] int serviceId,
            [FromQuery] DateTimeOffset date)
        {
            var service = await _context.services
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.id == serviceId &&
                    s.isActive &&
                    s.allowOnlineBooking);

            if (service == null)
            {
                return NotFound(new
                {
                    message = "Dịch vụ không tồn tại hoặc không cho phép đặt lịch trực tuyến"
                });
            }

            var availableTimes = await _context.shedules
                .AsNoTracking()
                .Where(x => x.serviceId == serviceId)
                .OrderBy(x => x.startTime)
                .Select(x => new
                {
                    x.id,
                    x.wordShift,
                    x.startTime,
                    x.endTime
                })
                .ToListAsync();

            return Ok(new
            {
                message = availableTimes.Count > 0
                    ? "Lấy danh sách khung giờ thành công"
                    : "Không có khung giờ nào",
                data = availableTimes,
                total = availableTimes.Count
            });
        }

        // GET: api/Appointments/time-slots
        [HttpGet("time-slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTimeSlots(
            [FromQuery] int serviceId,
            [FromQuery] DateTimeOffset date)
        {
            var service = await _context.services
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.id == serviceId &&
                    s.isActive &&
                    s.allowOnlineBooking);

            if (service == null)
            {
                return NotFound(new
                {
                    message = "Dịch vụ không tồn tại hoặc không cho phép đặt lịch trực tuyến"
                });
            }

            var schedules = await _context.shedules
                .AsNoTracking()
                .Where(x => x.serviceId == serviceId)
                .OrderBy(x => x.startTime)
                .ToListAsync();

            if (schedules.Count == 0)
            {
                return Ok(new
                {
                    message = "Dịch vụ chưa có lịch hoạt động.",
                    data = new List<object>()
                });
            }

            DateTimeOffset dayStart = new DateTimeOffset(
                date.Year,
                date.Month,
                date.Day,
                0, 0, 0,
                date.Offset);

            DateTimeOffset dayEnd = dayStart.AddDays(1);

            var bookings = await _context.bookings
                .AsNoTracking()
                .Where(b =>
                    b.serviceId == serviceId &&
                    b.Date >= dayStart &&
                    b.Date < dayEnd &&
                    b.status != "Cancelled")
                .Select(b => b.timeDate)
                .ToListAsync();

            var result = schedules.Select(schedule => new
            {
                schedule.wordShift,
                schedule.startTime,
                schedule.endTime,
                isAvailable = !bookings.Any(b =>
                    b >= schedule.startTime &&
                    b < schedule.endTime)
            }).ToList();

            return Ok(new
            {
                message = "Kiểm tra khung giờ thành công.",
                data = result
            });
        }

        // POST: api/Appointments/booking
        // Chỉ Guest đã đăng nhập mới được đặt lịch
        [HttpPost("booking")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> CreateAppointment(
            [FromBody] AppointmentRequest request)
        {
            // Lấy Guest ID từ JWT
            var guestIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(guestIdValue, out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Token không hợp lệ hoặc không tìm thấy Guest ID."
                });
            }

            // Kiểm tra Guest có tồn tại không
            var guest = await _context.guests
                .FirstOrDefaultAsync(x => x.id == guestId);

            if (guest == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản Guest."
                });
            }

            // Kiểm tra dịch vụ
            var service = await _context.services
                .FirstOrDefaultAsync(s =>
                    s.id == request.serviceId &&
                    s.isActive &&
                    s.allowOnlineBooking);

            if (service == null)
            {
                return BadRequest(new
                {
                    message = "Dịch vụ không hợp lệ."
                });
            }

            DateTimeOffset appointmentDate = request.appointmentDate;
            DateTimeOffset appointmentTime = request.appointmentTime;

            // Kiểm tra lịch hoạt động
            var schedules = await _context.shedules
                .Where(x => x.serviceId == request.serviceId)
                .ToListAsync();

            bool inWorkingTime = schedules.Any(schedule =>
                appointmentTime.TimeOfDay >= schedule.startTime.TimeOfDay &&
                appointmentTime.TimeOfDay < schedule.endTime.TimeOfDay);

            if (!inWorkingTime)
            {
                return BadRequest(new
                {
                    message = "Thời gian này không nằm trong lịch hoạt động."
                });
            }

            // Kiểm tra ngày đặt lịch
            DateTimeOffset dayStart = new DateTimeOffset(
                appointmentDate.Year,
                appointmentDate.Month,
                appointmentDate.Day,
                0, 0, 0,
                appointmentDate.Offset);

            DateTimeOffset dayEnd = dayStart.AddDays(1);

            bool alreadyBooked = await _context.bookings
                .AnyAsync(x =>
                    x.serviceId == request.serviceId &&
                    x.Date >= dayStart &&
                    x.Date < dayEnd &&
                    x.timeDate == appointmentTime &&
                    x.status != "Cancelled");

            if (alreadyBooked)
            {
                return BadRequest(new
                {
                    message = "Khung giờ này đã có người đặt."
                });
            }

            // Tạo booking cho Guest đang đăng nhập
            var booking = new Booking
            {
                guestId = guest.id,
                serviceId = request.serviceId,
                Date = appointmentDate,
                timeDate = appointmentTime,
                status = "Pending",
                sttId = string.Empty
            };

            _context.bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đặt lịch thành công.",
                data = new AppointmentResponse
                {
                    Id = booking.id,
                    GuestId = booking.guestId,
                    ServiceId = booking.serviceId,
                    ServiceName = service.serviceName,
                    Date = booking.Date,
                    TimeDate = booking.timeDate,
                    Status = booking.status,
                    SttId = booking.sttId
                }
            });
        }
    }
}
