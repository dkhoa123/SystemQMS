using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Models.DTOs;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/dsp")]
    public class DspController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DspController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet("central-screen")]
        public async Task<IActionResult> GetCentralScreenData([FromQuery] bool isShowQueue = false)
        {
            // Lấy danh sách các quầy đang mở. 
            // Giả định trường status của Counter là kiểu string và giá trị "Open" là đang mở (hoặc "Đang xử lý").
            var activeCounters = await _context.counters
                .Where(c => c.status == "Open")
                .Select(c => new CentralScreenDataDto
                {
                    CounterId = c.id,
                    CounterName = c.counterName,

                    // Lấy STT ĐANG XỬ LÝ của quầy này
                    // Trường 'status' trong Sticker lưu trạng thái (Đang chờ, Đang xử lý, Hoàn thành, Văng)
                    CurrentTicket = _context.stickers
                        .Where(s => s.counterId == c.id && s.status == "Processing")
                        .OrderByDescending(s => s.useTime) // Sắp xếp theo thời gian được gọi vào xử lý
                        .Select(s => s.sttId) // Lấy trường sttId làm chuỗi hiển thị số thứ tự
                        .FirstOrDefault(),

                    // Xử lý "Hiển thị hàng đợi" (DSP-01)[cite: 18]
                    WaitingTickets = isShowQueue
                        ? _context.stickers
                            .Where(s => s.counterId == c.id && s.status == "Waiting")
                            .OrderBy(s => s.issuanceTime) // Sắp xếp theo thời gian cấp số (cấp trước gọi trước)
                            .Take(5)
                            .Select(s => s.sttId) // Lấy trường sttId
                            .ToList()
                        : new List<string>()
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                Data = activeCounters
            });
        }
    }
}