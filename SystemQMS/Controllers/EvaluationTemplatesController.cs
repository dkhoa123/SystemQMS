using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.EvaluationsDto;

namespace SystemQMS.Controllers
{
    [Route("api/evaluation-templates")]
    [ApiController]
    public class EvaluationTemplatesController : ControllerBase
    {
        private readonly IEvaluationTemplateSvc _templateSvc;

        public EvaluationTemplatesController(IEvaluationTemplateSvc templateSvc)
        {
            _templateSvc = templateSvc;
        }

        // RTG-05: danh sách mẫu của đơn vị + tìm kiếm theo mã/tên
        [HttpGet]
        public async Task<ActionResult<List<EvaluationTemplateDto>>> GetTemplates(
            int unitId, string? keyword, CancellationToken ct)
        {
            return Ok(await _templateSvc.GetTemplatesAsync(unitId, keyword, ct));
        }

        // RTG-05: chi tiết mẫu
        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationTemplateDto>> GetTemplate(int id, CancellationToken ct)
        {
            try
            {
                return Ok(await _templateSvc.GetTemplateAsync(id, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // RTG-05: tạo mẫu
        [HttpPost]
        public async Task<ActionResult<EvaluationTemplateDto>> CreateTemplate(
            EvaluationTemplateUpsertDto dto, CancellationToken ct)
        {
            try
            {
                var created = await _templateSvc.CreateTemplateAsync(dto, ct);
                return CreatedAtAction(nameof(GetTemplate), new { id = created.id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // RTG-05: sửa mẫu (kèm sắp xếp câu/bật bắt buộc qua contentQuestion mới)
        [HttpPut("{id}")]
        public async Task<ActionResult<EvaluationTemplateDto>> UpdateTemplate(
            int id, EvaluationTemplateUpsertDto dto, CancellationToken ct)
        {
            try
            {
                return Ok(await _templateSvc.UpdateTemplateAsync(id, dto, ct));
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

        // RTG-05: xóa mẫu (mẫu đang áp dụng thì bị chặn)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTemplate(int id, CancellationToken ct)
        {
            try
            {
                await _templateSvc.DeleteTemplateAsync(id, ct);
                return NoContent();
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

        // RTG-05: nhân bản mẫu
        [HttpPost("{id}/clone")]
        public async Task<ActionResult<EvaluationTemplateDto>> CloneTemplate(int id, CancellationToken ct)
        {
            try
            {
                var clone = await _templateSvc.CloneTemplateAsync(id, ct);
                return CreatedAtAction(nameof(GetTemplate), new { id = clone.id }, clone);
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

        // RTG-05: cài đặt đánh giá — đặt mẫu đang áp dụng cho đơn vị
        [HttpPost("{id}/activate")]
        public async Task<ActionResult> ActivateTemplate(int id, CancellationToken ct)
        {
            try
            {
                await _templateSvc.ActivateTemplateAsync(id, ct);
                return Ok(new { message = "Đã áp dụng mẫu đánh giá" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
