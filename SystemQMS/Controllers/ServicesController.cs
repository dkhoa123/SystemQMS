using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceSvc _serviceSvc;

        public ServicesController(IServiceSvc serviceSvc)
        {
            _serviceSvc = serviceSvc;
        }


        // SVC-01: Danh sách dịch vụ (Lọc theo quầy xử lý, trạng thái, tìm mã/tên)

        [HttpGet("getAllServices")]
        public async Task<IActionResult> GetServices([FromQuery] FilterServiceDto filter)
        {
            var result = await _serviceSvc.GetAllServicesAsync(filter);
            return Ok(result);
        }


        // SVC-05: Xem chi tiết dịch vụ (Bao gồm quầy xử lý và danh sách STT đã cấp)

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceById(int id){
            var result = await _serviceSvc.GetServiceByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = $"Không tìm thấy dịch vụ có ID = {id}" });
            }
            return Ok(result);
        }

        [HttpGet("{id:int}/history")]
        public async Task<IActionResult> GetServiceHistory(int id, [FromQuery] FilterServiceDto filter)
        {
            var result = await _serviceSvc.GetServiceByIdAsync(id, filter);
            return Ok(result);
        }


        // SVC-02 & SVC-03: Thêm mới dịch vụ, thiết lập chế độ chia quầy & thời gian xử lý

        [HttpPost("create")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            //var unitIdClaim = User.FindFirst("unitId")?.Value;

            //if (!int.TryParse(unitIdClaim, out var unitId))
            //    return Unauthorized("Không tìm thấy thông tin đơn vị trong tài khoản.");

            var result = await _serviceSvc.CreateServiceAsync( 1, dto);

            return Ok(result);
        }


        // SVC-04: Cập nhật dịch vụ (Sửa toàn bộ trừ mã dịch vụ serviceId)

        [HttpPut("{id:int}/edit")]
        public async Task<IActionResult> UpdateService(int id,[FromBody] UpdateServiceDto dto)
        {
            var result = await _serviceSvc.UpdateServiceAsync(id, dto);
            return Ok(result);
        }


        // SVC-06: Xóa dịch vụ (Kiểm tra ràng buộc STT trước khi xóa)
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteService([FromBody] DeleteServiceDto dto)
        {
            var result = await _serviceSvc.DeleteServiceAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}