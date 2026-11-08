
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
    public class NewsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public NewsController(MyDbContext context)
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

        // GET: api/News
        // Danh sách tin tức dạng thẻ
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var unitId = await GetCurrentUnitId();

            if (unitId == null)
            {
                return BadRequest(new
                {
                    message = "Guest chưa thuộc phường nào."
                });
            }

            var now = DateTimeOffset.UtcNow;

            var news = await _context.news
                .AsNoTracking()
                .Where(n =>
                    n.unitId == unitId &&
                    n.status == "Published" &&
                    n.timeSend <= now)
                .OrderByDescending(n => n.timeSend)
                .Select(n => new
                {
                    n.Id,
                    n.title,
                    n.content,
                    n.timeSend,
                    n.status
                    
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy danh sách tin tức thành công.",
                total = news.Count,
                data = news
            });
        }

        // GET: api/News/1
        // Chi tiết tin tức
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var unitId = await GetCurrentUnitId();

            if (unitId == null)
            {
                return BadRequest(new
                {
                    message = "Guest chưa thuộc phường nào."
                });
            }

            var now = DateTimeOffset.UtcNow;

            var news = await _context.news
                .AsNoTracking()
                .Where(n =>
                    n.Id == id &&
                    n.unitId == unitId &&
                    n.status == "Published" &&
                    n.timeSend <= now)
                .Select(n => new
                {
                    n.Id,
                    n.title,
                    n.content,
                    n.timeSend,
                    n.status
                })
                .FirstOrDefaultAsync();

            if (news == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tin tức."
                });
            }

            return Ok(new
            {
                message = "Lấy chi tiết tin tức thành công.",
                data = news
            });
        }
    }
}
