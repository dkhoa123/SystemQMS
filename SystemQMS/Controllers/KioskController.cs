using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KioskController : ControllerBase
    {
        private IKioskSvc _kioskSvc;
        public KioskController(IKioskSvc kioskSvc)
        {
            _kioskSvc = kioskSvc;
        }

        [HttpGet("{kioskDeviceId}")]
        public IActionResult GetServiceKiosks(string kioskDeviceId)
        {
            var serviceKiosks = _kioskSvc.GetServiceKiosks(kioskDeviceId);
            return Ok(serviceKiosks);
        }

        [HttpPost("ticket/{serviceId}")]
        public async Task<IActionResult> GetTicket(int serviceId, int counterId, int userId)
        {
            var ticket = await _kioskSvc.CreateTicketAsync(serviceId, counterId, userId);
            return Ok(ticket);
        }

        [HttpPost("report-device-status")]
        public IActionResult ReportDeviceStatus(string kioskDeviceId, string status)
        {
            var result = _kioskSvc.ReportDeviceStatus(kioskDeviceId, status);
            return Ok(result);
        }
    }
}
