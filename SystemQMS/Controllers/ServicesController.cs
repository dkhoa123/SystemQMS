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

        [HttpGet]
        public async Task<IActionResult> GetServices(
            [FromQuery] int? unitId,
            [FromQuery] int? counterId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search)
        {
            var result = await _serviceSvc.GetAllServicesAsync(unitId, counterId, isActive, search);
            return Ok(result);
        }


        // SVC-05: Xem chi tiết dịch vụ (Bao gồm quầy xử lý và danh sách STT đã cấp)

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceById(
            int id,
            [FromQuery] string? ticketStatus,
            [FromQuery] DateTimeOffset? fromDate,
            [FromQuery] DateTimeOffset? toDate)
        {
            var result = await _serviceSvc.GetServiceByIdAsync(id, ticketStatus, fromDate, toDate);
            if (result == null)
            {
                return NotFound(new { message = $"Không tìm thấy dịch vụ có ID = {id}" });
            }
            return Ok(result);
        }

   
        // SVC-02 & SVC-03: Thêm mới dịch vụ, thiết lập chế độ chia quầy & thời gian xử lý

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _serviceSvc.CreateServiceAsync(dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetServiceById), new { id = result.NewId }, new 
            { 
                id = result.NewId, 
                serviceId = result.ServiceId, 
                message = result.Message 
            });
        }


        // SVC-04: Cập nhật dịch vụ (Sửa toàn bộ trừ mã dịch vụ serviceId)

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _serviceSvc.UpdateServiceAsync(id, dto);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }


        // SVC-06: Xóa dịch vụ (Kiểm tra ràng buộc STT trước khi xóa)

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var result = await _serviceSvc.DeleteServiceAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}