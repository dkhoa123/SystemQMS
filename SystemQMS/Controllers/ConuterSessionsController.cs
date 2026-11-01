using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CounterSessionDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConuterSessionsController : ControllerBase
    {
        private ICounterSessionSvc _counter;
        public ConuterSessionsController (ICounterSessionSvc counter)
        {
            _counter = counter;
        }

        [HttpPost("take-control")]
        public async Task<IActionResult> TakeControl([FromBody] CounterSessionDto model)
        {
            var result = await _counter.TakeControl(model);
            if (result != "Nhận quyền điều khiển quầy thành công")
                return BadRequest(new { message = result });
            return Ok(new { message = result });

        }

        [HttpPut("release-control")]
        public async Task<IActionResult> ReleaseControl(int counterId , int userId)
        {
            var result = await _counter.ReleaseControl(counterId, userId);
            if (result != "Trả quyền điều khiển thành công")
                return BadRequest(new { message = result });
            return Ok(new { message = result });

        }
    }
}
