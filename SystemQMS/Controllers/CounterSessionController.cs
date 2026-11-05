using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/counters/{id:int}")]
    public class CounterSessionController : ControllerBase
    {
        private readonly ICounterSessionSvc _sessionSvc;

        public CounterSessionController(ICounterSessionSvc sessionSvc)
        {
            _sessionSvc = sessionSvc;
        }

        [HttpPost("session")]
        public async Task<IActionResult> OpenSession(int id, [FromBody] OpenSessionRequestDto dto)
        {
            var result = await _sessionSvc.OpenSessionAsync(id, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpGet("session")]
        public async Task<IActionResult> GetSession(int id)
        {
            var result = await _sessionSvc.GetCurrentSessionAsync(id);
            if (result == null) return NotFound(new { message = $"Không tìm thấy quầy có ID = {id}" });
            return Ok(result);
        }

        [HttpPost("session/close")]
        public async Task<IActionResult> CloseSession(int id, [FromQuery] int userId)
        {
            var result = await _sessionSvc.CloseSessionAsync(id, userId);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseCounter(int id)
        {
            var result = await _sessionSvc.CloseCounterAsync(id);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("open")]
        public async Task<IActionResult> OpenCounter(int id)
        {
            var result = await _sessionSvc.OpenCounterAsync(id);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}