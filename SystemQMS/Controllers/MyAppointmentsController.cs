
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SystemQMS.Models;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Guest")]
    public class MyAppointmentsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public MyAppointmentsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/MyAppointments
        // Danh sách lịch của Guest đang đăng nhập
        [HttpGet]
        public async Task<IActionResult> GetMyAppointments()
        {
            var guestIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(guestIdValue, out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Token không hợp lệ."
                });
            }

            var appointments = await _context.bookings
                .AsNoTracking()
                .Where(b => b.guestId == guestId)
                .OrderByDescending(b => b.Date)
                .ThenByDescending(b => b.timeDate)
                .Select(b => new
                {
                    id = b.id,
                    registrationCode = b.sttId,
                    serviceId = b.serviceId,
                    serviceName = b.Service.serviceName,
                    date = b.Date,
                    time = b.timeDate,
                    status = b.status
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy danh sách lịch hẹn thành công.",
                total = appointments.Count,
                data = appointments
            });
        }

        // GET: api/MyAppointments/1
        // Chi tiết một lịch hẹn
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentDetail(int id)
        {
            var guestIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(guestIdValue, out int guestId))
            {
                return Unauthorized(new
                {
                    message = "Token không hợp lệ."
                });
            }

            var appointment = await _context.bookings
                .AsNoTracking()
                .Where(b => b.id == id && b.guestId == guestId)
                .Select(b => new
                {
                    id = b.id,
                    registrationCode = b.sttId,
                    serviceId = b.serviceId,
                    serviceName = b.Service.serviceName,
                    date = b.Date,
                    time = b.timeDate,
                    status = b.status,

                    // Frontend dùng nội dung này để tạo QR
                    qrContent = b.sttId
                })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy lịch hẹn."
                });
            }

            return Ok(new
            {
                message = "Lấy chi tiết lịch hẹn thành công.",
                data = appointment
            });
        }
    }
}
