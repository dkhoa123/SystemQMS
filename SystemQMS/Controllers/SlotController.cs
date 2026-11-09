using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.IntergrationsDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/slots")]
    public class SlotController : ControllerBase
    {
        private readonly ISlotSvc _slotSvc;

        public SlotController(ISlotSvc slotSvc)
        {
            _slotSvc = slotSvc;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveSlot([FromBody] ReserveSlotRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int currentUnitId = 1;

            var result = await _slotSvc.ReserveSlotAsync(currentUnitId, request);

            if (result.IsSuccess)
            {
                return Ok(new { success = true, message = result.Message, bookingId = result.BookingId });
            }

            return BadRequest(new { success = false, message = result.Message });
        }
    }
}