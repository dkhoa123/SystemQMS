using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interfaces;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/integrations")]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationSvc _integrationSvc;

        public IntegrationController(IIntegrationSvc integrationSvc)
        {
            _integrationSvc = integrationSvc;
        }

        [HttpGet("crm-customers")]
        public async Task<IActionResult> GetCrmCustomers([FromQuery] string keyword = null)
        {
            int currentUnitId = 1;

            var customers = await _integrationSvc.GetCrmCustomersAsync(currentUnitId, keyword);
            return Ok(new { success = true, data = customers });
        }

        [HttpGet("qms-services")]
        public async Task<IActionResult> GetQmsServices()
        {
            int currentUnitId = 1;

            var services = await _integrationSvc.GetQmsServicesAsync(currentUnitId);
            return Ok(new { success = true, data = services });
        }
    }
}