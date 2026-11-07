using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CountersDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/counters/{id:int}/tickets")]
    public class CounterTicketController : ControllerBase
    {
        private readonly ICounterTicketSvc _ticketSvc;

        public CounterTicketController(ICounterTicketSvc ticketSvc)
        {
            _ticketSvc = ticketSvc;
        }

        [HttpPost("call")]
        public async Task<IActionResult> CallNext(int id, [FromQuery] int userId)
        {
            var result = await _ticketSvc.CallNextTicketAsync(id, userId);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPost("call-multiple")]
        public async Task<IActionResult> CallMultiple(int id, [FromQuery] int userId)
        {
            var result = await _ticketSvc.CallMultipleTicketsAsync(id, userId);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPost("{sttId}/complete")]
        public async Task<IActionResult> Complete(int id, string sttId, [FromQuery] int userId)
        {
            var result = await _ticketSvc.CompleteTicketAsync(id, sttId, userId);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPost("{sttId}/skip")]
        public async Task<IActionResult> Skip(int id, string sttId, [FromQuery] int userId, [FromBody] SkipTicketRequestDto? dto)
        {
            var result = await _ticketSvc.SkipTicketAsync(id, sttId, userId, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [HttpPost("{sttId}/recall")]
        public async Task<IActionResult> Recall(int id, string sttId, [FromQuery] int userId)
        {
            var result = await _ticketSvc.RecallTicketAsync(id, sttId, userId);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
    }
}