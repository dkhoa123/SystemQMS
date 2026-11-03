using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DspDto;

namespace SystemQMS.System.Controllers
{
    [ApiController]
    [Route("api/v1/dsp")]
    public class DspController : ControllerBase
    {
        private readonly IDspSvc _dspService;

        public DspController(IDspSvc dspService)
        {
            _dspService = dspService;
        }

        [HttpGet("main-display/state")]
        public async Task<ActionResult<MainDisplayResponseDto>> GetMainDisplayState()
        {
            var result = await _dspService.GetMainDisplayStateAsync();
            return Ok(result);
        }

        [HttpGet("counters/{counterId}/state")]
        public async Task<ActionResult<CounterDisplayResponseDto>> GetCounterDisplayState(int counterId)
        {
            var result = await _dspService.GetCounterDisplayStateAsync(counterId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost("devices/{deviceId}/logs")]
        public async Task<IActionResult> LogDeviceError(string deviceId, [FromBody] ErrorLogRequestDto error)
        {
            await _dspService.LogDeviceErrorAsync(deviceId, error);
            return Ok();
        }
    }
}