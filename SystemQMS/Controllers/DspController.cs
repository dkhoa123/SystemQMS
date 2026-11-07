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
    }
}