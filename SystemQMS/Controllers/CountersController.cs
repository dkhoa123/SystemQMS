using Microsoft.AspNetCore.Mvc;
using SystemQMS.Dtos;
using SystemQMS.Services;
using SystemQMS.Services.Interface;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountersController : ControllerBase
    {
        private readonly ICounterSvc _counterSvc;
        public CountersController(ICounterSvc counterSvc)
        {
            _counterSvc = counterSvc;
        }

        // CNT-01
        [HttpGet]
        public async Task<ActionResult<List<CounterResponse>>> GetCounters(
            int unitId, string? status = null, bool? isActive = null, string? search = null)
        {
            return await _counterSvc.GetCountersAsync(unitId, status, isActive, search);
        }

        // CNT-02
        [HttpPost]
        public async Task<ActionResult<CounterResponse>> CreateCounter(CreateCounterRequest request)
        {
            try
            {
                var created = await _counterSvc.CreateCounterAsync(request);
                return Created($"/api/counters/{created.id}", created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // CNT-03
        [HttpPut("{id}")]
        public async Task<ActionResult<CounterResponse>> UpdateCounter(int id, UpdateCounterRequest request)
        {
            try
            {
                return Ok(await _counterSvc.UpdateCounterAsync(id, request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // CNT-04a
        [HttpGet("{id}")]
        public async Task<ActionResult<CounterDetailResponse>> GetCounter(int id)
        {
            try
            {
                return Ok(await _counterSvc.GetCounterAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // CNT-04b
        [HttpGet("{id}/stickers")]
        public async Task<ActionResult<List<CounterStickerResponse>>> GetCounterStickers(
            int id, string? status = null, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            try
            {
                return Ok(await _counterSvc.GetCounterStickersAsync(id, status, from, to));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // CNT-05
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCounter(int id, CancellationToken ct)
        {
            try
            {
                await _counterSvc.DeleteCounterAsync(id, ct);
                return NoContent(); // 204
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