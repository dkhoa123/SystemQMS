using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.WebConfigDto;
//using System.Security.Claims;


namespace SystemQMS.Controllers
{
    [Route("api/web-configs")]
    [ApiController]
    //[Authorize]
    public class WebConfigController : ControllerBase
    {
        private readonly IWebConfigSvc _webConfigService;

        public WebConfigController(IWebConfigSvc webConfigService)
        {
            _webConfigService = webConfigService;
        }

        private int GetCurrentUnitId()
        {
            // var unitIdClaim = User.FindFirst("UnitId")?.Value;
            // return int.Parse(unitIdClaim);

            return 1; // Giả định để test
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            int unitId = GetCurrentUnitId();
            var config = await _webConfigService.GetWebConfigAsync(unitId);

            if (config == null)
                return NotFound(new { message = "Không tìm thấy cấu hình cho đơn vị này." });

            return Ok(new { data = config });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] WebConfigDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int unitId = GetCurrentUnitId();

            try
            {
                var result = await _webConfigService.UpdateWebConfigAsync(unitId, dto);

                if (!result)
                    return BadRequest(new { message = "Lỗi khi cập nhật cấu hình. Đơn vị không tồn tại." });

                return Ok(new { message = "Lưu cấu hình Web App thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ.", error = ex.Message });
            }
        }

        [HttpPost("uploads")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string type = "images")
        {
            try
            {
                var fileUrl = await _webConfigService.UploadFileAsync(file, type);
                return Ok(new { url = fileUrl, message = "Upload file thành công." });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ khi upload file.", error = ex.Message });
            }
        }
    }
}