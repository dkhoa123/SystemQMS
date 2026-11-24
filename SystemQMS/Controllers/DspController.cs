using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.AudioDto;
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
        [HttpPost("counters/{counterId:int}/play-audio")]
        public async Task<ActionResult<AudioCallEventDto>> PlayCallAudio(
    int counterId,
    [FromQuery] string? sttId,
    [FromQuery] string? ticketNumber,
    [FromQuery] int? repeatCount,
    [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] PlayAudioRequestDto? body)
        {
            var resolvedIdentifier = sttId
                ?? body?.SttId
                ?? ticketNumber
                ?? body?.TicketNumber;

            if (string.IsNullOrWhiteSpace(resolvedIdentifier))
            {
                return BadRequest(new { message = "Thiếu tham số sttId hoặc ticketNumber." });
            }

            int finalRepeat = repeatCount ?? body?.RepeatCount ?? 1;
            finalRepeat = Math.Clamp(finalRepeat, 1, 3);

            var result = await _dspService.TriggerCallAudioAsync(
                GetCurrentUnitId(),
                counterId,
                resolvedIdentifier,
                finalRepeat);

            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy quầy hoặc thông tin vé tương ứng với sttId." });
            }

            return Ok(result);
        }
    }

}