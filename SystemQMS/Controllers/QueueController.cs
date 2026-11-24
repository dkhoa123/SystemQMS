using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.QueueDto;

namespace SystemQMS.Controllers
{
    [Route("api/Counters/{counterId}/queue")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly IQueueSvc _queueSvc;

        public QueueController(IQueueSvc queueSvc)
        {
            _queueSvc = queueSvc;
        }

        //[HttpPost("pause")]
        //public async Task<ActionResult<CounterStateResponse>> Pause(int counterId, CancellationToken ct)
        //{
        //    try
        //    {
        //        return Ok(await _queueSvc.PauseAcceptingAsync(counterId, ct));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //[HttpPost("resume")]
        //public async Task<ActionResult<CounterStateResponse>> Resume(int counterId, CancellationToken ct)
        //{
        //    try
        //    {
        //        return Ok(await _queueSvc.ResumeAcceptingAsync(counterId, ct));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpGet("tickets")]
        public IActionResult GetTickets(int counterId)
        {
            try
            {
                return Ok(_queueSvc.getStatesCounter(counterId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("tickets/filter")]
        public IActionResult GetFilteredTickets(int counterId, [FromQuery] FilterQueueDto filter)
        {
            try
            {
                return Ok(_queueSvc.GetCounterTickets(filter));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public IActionResult GetHistory(int counterId)
        {
            try
            {
                return Ok(_queueSvc.GetCounterHistory(counterId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("call")]
        public IActionResult Call(int counterId, [FromBody] CallRequest req)
        {
            try
            {
                // Frontend gửi sttId thì gọi đúng số đó
                if (!string.IsNullOrWhiteSpace(req.SttId))
                    return Ok(_queueSvc.CallTicket(counterId, req.SttId));

                // Không có sttId thì gọi N số kế tiếp (tối đa 6)
                var count = Math.Clamp(req.Count, 1, 6);
                return Ok(_queueSvc.CallNext(counterId, count));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        //[HttpPost("/api/counters/{counterId}/toggle-receiving")]
        //public async Task<ActionResult<CounterStateResponse>> ToggleReceiving(int counterId, CancellationToken ct)
        //{
        //    try
        //    {
        //        return Ok(await _queueSvc.ToggleReceivingAsync(counterId, ct));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //[HttpPost("/api/stickers/{sttId}/priority")]
        //public async Task<ActionResult<QueueTicketDto>> SetPriority(string sttId, CancellationToken ct)
        //{
        //    try
        //    {
        //        return Ok(await _queueSvc.SetPriorityAsync(sttId, ct));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}
