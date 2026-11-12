using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationsController : ControllerBase
    {
        private readonly IEvaluationSvc _evaluationSvc;

        public EvaluationsController(IEvaluationSvc evaluationSvc)
        {
            _evaluationSvc = evaluationSvc;
        }


        // RTG-01 + RTG-03: danh sách + tìm kiếm/lọc (cố định 10 dòng/trang)
        [HttpGet]
        public async Task<ActionResult<EvaluationListResponse>> GetEvaluations(
            int unitId, int page = 1, string? keyword = null, [FromQuery] List<int>? counterIds = null,
            DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
        {
            return Ok(await _evaluationSvc.GetEvaluationsAsync(
                unitId, page, keyword, counterIds, from, to, ct));
        }

        // RTG-02: chi tiết
        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationDetailDto>> GetEvaluation(int id, CancellationToken ct)
        {
            try
            {
                return Ok(await _evaluationSvc.GetEvaluationAsync(id, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // RTG-04 phải khai trước {id} để "export" không bị nuốt thành id
        [HttpGet("export")]
        public async Task<ActionResult> Export(
            int unitId, string? keyword, [FromQuery] List<int>? counterIds,
            DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
        {
            var bytes = await _evaluationSvc.ExportEvaluationsAsync(unitId, keyword, counterIds, from, to, ct);
            var fileName = $"danh-gia_{DateTimeOffset.Now:yyyyMMdd-HHmm}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // RTG-06: tra cứu STT cho popup tự điền (STT không tồn tại vẫn 200)
        [HttpGet("lookup/{sttId}")]
        public async Task<ActionResult<EvaluationLookupDto>> Lookup(
            string sttId, int unitId, CancellationToken ct)
        {
            return Ok(await _evaluationSvc.LookupAsync(sttId, unitId, ct));
        }

        // RTG-06/07: gửi đánh giá từ Kiosk
        [HttpPost]
        public async Task<ActionResult<EvaluationDetailDto>> Submit(
            EvaluationSubmitDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _evaluationSvc.SubmitAsync(dto, ct);
                return CreatedAtAction(nameof(GetEvaluation), new { id = created.id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // RTG-08: dữ liệu màn hình cho thiết bị đánh giá tại quầy
        // (truyền counterId, hoặc deviceId để thiết bị tự resolve quầy của nó)
        [HttpGet("device-screen")]
        public async Task<ActionResult<RatingScreenDto>> DeviceScreen(
            int? counterId, string? deviceId, CancellationToken ct)
        {
            try
            {
                return Ok(await _evaluationSvc.GetDeviceScreenAsync(counterId, deviceId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
