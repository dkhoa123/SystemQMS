using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.ScheduleDto;

namespace SystemQMS.Controllers
{
    [Route("api/schedules")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleSvc _scheduleSvc;

        public SchedulesController(IScheduleSvc scheduleSvc)
        {
            _scheduleSvc = scheduleSvc;
        }

        // HRS-01: dịch vụ có đang trong giờ + còn quầy mở phục vụ không
        [HttpGet("check-active")]
        public async Task<ActionResult<ServiceAvailabilityResponse>> CheckActive(
            int serviceId, CancellationToken ct)
        {
            try
            {
                return Ok(await _scheduleSvc.CheckServiceAvailabilityAsync(serviceId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}