using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.QueueDto;

namespace SystemQMS.Controllers
{
    [Route("api/counters/{counterId}/queue")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly IQueueSvc _queueSvc;

        public QueueController(IQueueSvc queueSvc)
        {
            _queueSvc = queueSvc;
        }

        [HttpPost("pause")]
        public async Task<ActionResult<CounterStateResponse>> Pause(int counterId, CancellationToken ct)
        {
            try
            {
                return Ok(await _queueSvc.PauseAcceptingAsync(counterId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("resume")]
        public async Task<ActionResult<CounterStateResponse>> Resume(int counterId, CancellationToken ct)
        {
            try
            {
                return Ok(await _queueSvc.ResumeAcceptingAsync(counterId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("tickets")]
        public async Task<ActionResult<CounterTicketsResponse>> GetTickets(int counterId, CancellationToken ct)
        {
            try
            {
                return Ok(await _queueSvc.GetCounterTicketsAsync(counterId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<CounterHistoryResponse>> GetHistory(
            int counterId, int page = 1, int pageSize = 20,
            DateTimeOffset? from = null, DateTimeOffset? to = null,
            CancellationToken ct = default)
        {
            try
            {
                return Ok(await _queueSvc.GetCounterHistoryAsync(counterId, page, pageSize, from, to, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost("/api/counters/{counterId}/toggle-receiving")]
        public async Task<ActionResult<CounterStateResponse>> ToggleReceiving(int counterId, CancellationToken ct)
        {
            try
            {
                return Ok(await _queueSvc.ToggleReceivingAsync(counterId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("/api/stickers/{sttId}/priority")]
        public async Task<ActionResult<QueueTicketDto>> SetPriority(string sttId, CancellationToken ct)
        {
            try
            {
                return Ok(await _queueSvc.SetPriorityAsync(sttId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
