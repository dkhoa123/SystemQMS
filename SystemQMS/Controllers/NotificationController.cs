using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interfaces;
using SystemQMS.ViewModels.NotificationDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    // [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationSvc _notificationSvc;

        public NotificationController(INotificationSvc notificationSvc)
        {
            _notificationSvc = notificationSvc;
        }

        private int GetCurrentUnitId()
        {
            return 1; //test
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var unitId = GetCurrentUnitId();
            var result = await _notificationSvc.GetAllAsync(unitId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var unitId = GetCurrentUnitId();
            var result = await _notificationSvc.GetByIdAsync(id, unitId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy thông báo." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var unitId = GetCurrentUnitId();
                var result = await _notificationSvc.CreateAsync(unitId, dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NotificationUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var unitId = GetCurrentUnitId();
                var success = await _notificationSvc.UpdateAsync(id, unitId, dto);
                if (!success)
                    return NotFound(new { message = "Không tìm thấy thông báo." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var unitId = GetCurrentUnitId();
                var success = await _notificationSvc.DeleteAsync(id, unitId);
                if (!success)
                    return NotFound(new { message = "Không tìm thấy thông báo." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/retry")]
        public async Task<IActionResult> RetryFailed(int id)
        {
            var unitId = GetCurrentUnitId();
            var success = await _notificationSvc.RetryFailedAsync(id, unitId);

            if (!success)
                return BadRequest(new { message = "Không có người nhận bị lỗi hoặc thông báo không tồn tại." });

            return Ok(new { message = "Đã đưa người nhận bị lỗi vào hàng đợi chờ gửi lại." });
        }
    }
}