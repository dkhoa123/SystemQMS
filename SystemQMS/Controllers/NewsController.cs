using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemQMS.Services.Interfaces;
using SystemQMS.ViewModels.NewsDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class NewsController : ControllerBase
    {
        private readonly INewsSvc _newsSvc;

        public NewsController(INewsSvc newsSvc)
        {
            _newsSvc = newsSvc;
        }

        /*
        private int CurrentUnitId => int.Parse(User.FindFirstValue("UnitId") ?? "0");

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        */
        private int CurrentUnitId => 1;
        private int CurrentUserId => 1;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string status = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (data, totalCount) = await _newsSvc.GetAllAsync(CurrentUnitId, status, page, pageSize);
            return Ok(new { Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _newsSvc.GetByIdAsync(id, CurrentUnitId);
            if (news == null) return NotFound(new { message = "Không tìm thấy tin tức hoặc không có quyền truy cập." });

            return Ok(news);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewsCreateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdNews = await _newsSvc.CreateAsync(request, CurrentUnitId, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = createdNews.Id }, createdNews);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NewsUpdateDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isUpdated = await _newsSvc.UpdateAsync(id, request, CurrentUnitId, CurrentUserId);
            if (!isUpdated) return NotFound(new { message = "Không tìm thấy tin tức để cập nhật." });

            return Ok(new { message = "Cập nhật thành công." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _newsSvc.DeleteAsync(id, CurrentUnitId);
            if (!isDeleted) return NotFound(new { message = "Không tìm thấy tin tức để xóa." });

            return Ok(new { message = "Xóa thành công." });
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest(new { message = "Danh sách ID không hợp lệ." });

            var isDeleted = await _newsSvc.BulkDeleteAsync(ids, CurrentUnitId);
            if (!isDeleted) return NotFound(new { message = "Không có dữ liệu nào được xóa." });

            return Ok(new { message = $"Đã xóa thành công các tin tức đã chọn." });
        }
    }
}