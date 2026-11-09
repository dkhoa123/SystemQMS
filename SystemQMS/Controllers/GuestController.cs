using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CrmDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/crm/guests")]
    public class GuestController : ControllerBase
    {
        private readonly IGuestSvc _guestSvc;

        public GuestController(IGuestSvc guestSvc)
        {
            _guestSvc = guestSvc;
        }

        [HttpGet]
        public async Task<IActionResult> GetGuests([FromQuery] GuestQueryFilterDto filter)
        {
            var result = await _guestSvc.GetGuestsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuestById(int id)
        {
            var guest = await _guestSvc.GetGuestByIdAsync(id);
            if (guest == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng với id = {id}." });
            }
            return Ok(guest);
        }

        [HttpGet("cccd/{cccd}")]
        public async Task<IActionResult> GetGuestByCccd(string cccd)
        {
            var guest = await _guestSvc.GetGuestByCccdAsync(cccd);
            if (guest == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng với số CCCD = {cccd}." });
            }
            return Ok(guest);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _guestSvc.CreateGuestAsync(request);
                return CreatedAtAction(nameof(GetGuestById), new { id = created.id }, created);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _guestSvc.UpdateGuestAsync(id, request);
                return Ok(updated);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            try
            {
                await _guestSvc.DeleteGuestAsync(id);
                return Ok(new { message = "Đã vô hiệu hóa tài khoản khách hàng thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}