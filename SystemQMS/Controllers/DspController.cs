using Microsoft.AspNetCore.Mvc;
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

        private int GetCurrentUnitId() => 1;

        [HttpGet("main-display/page")]
        public async Task<ActionResult<DisplayBoardPageResponseDto>> GetMainDisplayPage(
            [FromQuery] int page = 1,
            [FromQuery] bool showWaitlist = true)
        {
            var result = await _dspService.GetDisplayBoardPageAsync(page, showWaitlist, GetCurrentUnitId());
            return Ok(result);
        }

        [HttpGet("main-display/counter-status")]
        public async Task<ActionResult<DisplayCounterStatusResponseDto>> GetCounterStatus()
        {
            var result = await _dspService.GetDisplayCounterStatusAsync(GetCurrentUnitId());
            return Ok(result);
        }

        [HttpGet("counters/{counterId:int}/screen")]
        public async Task<ActionResult<CounterScreenStateResponseDto>> GetCounterScreenState(
            int counterId,
            [FromQuery] bool showWaitlist = true)
        {
            var result = await _dspService.GetCounterScreenStateAsync(counterId, showWaitlist);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("counters/{counterId:int}/multi-call")]
        public async Task<ActionResult<CounterMultiCallDisplayResponseDto>> GetCounterMultiCallDisplay(
            int counterId,
            [FromQuery] int waitingLimit = 9)
        {
            var result = await _dspService.GetCounterMultiCallDisplayAsync(counterId, waitingLimit);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("devices/{deviceId}/display-errors")]
        public async Task<ActionResult<DisplayMaintenanceResponseDto>> ReportDisplayError(
            string deviceId,
            [FromBody] DisplayErrorRequestDto request)
        {
            var result = await _dspService.ReportDisplayErrorAndSwitchMaintenanceAsync(deviceId, request);
            return Ok(result);
        }
    }
}