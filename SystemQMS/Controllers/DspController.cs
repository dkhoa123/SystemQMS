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
        // Giả lập service xử lý logic database
        private readonly IDspSvc _dspService;

        public DspController(IDspSvc dspService)
        {
            _dspService = dspService;
        }

        // Khởi tạo Màn hình trung tâm (DSP-01, DSP-02)
        [HttpGet("main-display/state")]
        public async Task<ActionResult<MainDisplayResponseDto>> GetMainDisplayState()
        {
            var result = await _dspService.GetMainDisplayStateAsync();
            return Ok(result);
        }

        // Khởi tạo Màn hình quầy (DSP-03, DSP-04)
        [HttpGet("counters/{counterId}/state")]
        public async Task<ActionResult<CounterDisplayResponseDto>> GetCounterDisplayState(int counterId)
        {
            var result = await _dspService.GetCounterDisplayStateAsync(counterId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        // Thiết bị báo lỗi kết nối/hiển thị (DSP-06)
        [HttpPost("devices/{deviceId}/logs")]
        public async Task<IActionResult> LogDeviceError(string deviceId, [FromBody] ErrorLogRequestDto error)
        {
            await _dspService.LogDeviceErrorAsync(deviceId, error);
            return Ok(); // Client sẽ chuyển sang màn hình bảo trì sau khi gọi API này
        }
    }
}