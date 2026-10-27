using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Models;
using SystemQMS.Services;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumberFomularController : ControllerBase
    {
        private INumberFomularSvc _number;
        public NumberFomularController(INumberFomularSvc number)
        {
            _number = number;
        }

        [HttpPost]
        public IActionResult AddNew([FromBody] NumberFomular numberFomular)
        {
            if (numberFomular == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");

            }
            if (numberFomular.startNumber < 0 || numberFomular.endNumber < numberFomular.startNumber)
            {
                return BadRequest("Phạm vi không hợp lệ");

            }
            return Ok(new { NotificationStatus = _number.AddNew(numberFomular) });
        }

        [HttpGet("All")]
        public IActionResult GetNumberFomuilar()
        {
            return Ok(_number.GetNumberFomular());
        }

        [HttpPut]
        public IActionResult Update(int id, NumberFomular numberFomular)
        {
            if (id != numberFomular.id)
                return BadRequest("ID không hợp lệ");
            var result = _number.Update(numberFomular);
            if (result == null)
                return NotFound("Không tìm thấy quy tắc cấp số");
            return Ok("Cập nhật thành công");
        }

        [HttpGet("Preview/{serviceId}")]
        // 
        public IActionResult Preview(int serviceId)
        {
            // kiem tra dich vu, tao mau
            var result = _number.Preview(serviceId);
            // kiem tra qts
            if(result == null)
            {
                return NotFound("Dịch vụ chưa có quy tắc cấp số.");
            }
            return Ok(new
            {
                ServiceId = serviceId,
                PreviewNumber = result
            });
        }

        [HttpPost("covert-to-shared")]
        public async Task<IActionResult> ConvertToShared([FromBody] List<int> serviceIds)
        {
            if (serviceIds == null || serviceIds.Count == 0)
                return BadRequest("Danh sách dịch vụ không được rỗng .");
            var result = await _number.ConvertToShared(serviceIds);
            if (!result)
                return BadRequest("Không thể chuyển đổi.Hãy kiểm tra quy tắc cấp số và chế độ hiện tại.");
            return Ok("Chuyển sang chế độ thành công");

        }


        [HttpPost("reset")]
        public IActionResult Reset(int id)
        {
            var result = _number.ReSet(id);
            if (result == null)
                return BadRequest("Reset thất bại");
            return Ok("Reset thành công");
        }


    }
}
