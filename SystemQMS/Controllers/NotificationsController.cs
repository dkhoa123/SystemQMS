
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
    public class NotificationsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NotificationsController(MyDbContext context)
        {
            _context = context;
        }

        // Lấy unitId của Guest đang đăng nhập
        private async Task<int?> GetCurrentUnitId()
        {
            var guestIdValue = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(guestIdValue, out int guestId))
                return null;

            return await _context.guests
                .Where(g => g.id == guestId)
                .Select(g => g.unitId)
                .FirstOrDefaultAsync();
        }

        // GET: api/Notifications
        // Danh sách thông báo
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var unitId = await GetCurrentUnitId();

            if (unitId == null)
                return BadRequest(new
                {
                    message = "Guest chưa thuộc đơn vị nào."
                });

            var data = await _context.notifications
                .AsNoTracking()
                .Where(n =>
                    n.unitId == unitId &&
                    n.status == "Published" &&
                    n.timeSend <= DateTimeOffset.UtcNow)
                .OrderByDescending(n => n.timeSend)
                .Select(n => new
                {
                    n.Id,
                    n.type,
                    n.title,
                    n.content,
                    n.form,
                    n.timeSend,
                    n.status
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy danh sách thông báo thành công.",
                total = data.Count,
                data
            });
        }

        // GET: api/Notifications/1
        // Chi tiết thông báo
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var unitId = await GetCurrentUnitId();

            if (unitId == null)
                return BadRequest(new
                {
                    message = "Guest chưa thuộc đơn vị nào."
                });

            var notification = await _context.notifications
                .AsNoTracking()
                .Where(n =>
                    n.Id == id &&
                    n.unitId == unitId &&
                    n.status == "Published" &&
                    n.timeSend <= DateTimeOffset.UtcNow)
                .Select(n => new
                {
                    n.Id,
                    n.type,
                    n.title,
                    n.content,
                    n.form,
                    n.timeSend,
                    n.status
                })
                .FirstOrDefaultAsync();

            if (notification == null)
                return NotFound(new
                {
                    message = "Không tìm thấy thông báo."
                });

            return Ok(new
            {
                message = "Lấy chi tiết thông báo thành công.",
                data = notification
            });
        }
    }
}
