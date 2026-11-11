using Microsoft.AspNetCore.Mvc;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.NumberFomular;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumberFomularController : ControllerBase
    {
        private readonly INumberFomularSvc _numberFomularSvc;

        public NumberFomularController(INumberFomularSvc numberFomularSvc)
        {
            _numberFomularSvc = numberFomularSvc;
        }

        // GET: api/NumberFomular/
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _numberFomularSvc.GetAll();
            return Ok(result);
        }

        // POST: api/NumberFomular
        [HttpPost]
        public IActionResult AddNew([FromBody] CreateNumberFomularDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _numberFomularSvc.AddNew(dto);

            if (result == "Tạo quy tắc cấp số thành công.")
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }

        // PUT: api/NumberFomular
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateNumberFomularDto dto)
        {
            var result = await _numberFomularSvc.Update(id, dto);

            if (result == "Không tìm thấy công thức.")
                return NotFound(new { message = result });

            if (result != "Cập nhật công thức thành công.")
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }

        // GET: api/NumberFomular/Preview/4
        [HttpGet("Preview/{serviceId}")]
        public async Task<IActionResult> Preview(int serviceId)
        {
            try
            {
                var result = await _numberFomularSvc.Preview(serviceId);

                return Ok(new
                {
                    success = true,
                    preview = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        // POST: api/NumberFomular/ConvertToShared
        //[HttpPost("ConvertToShared")]
        //public async Task<IActionResult> ConvertToShared([FromBody] List<int> serviceIds)
        //{
        //    if (serviceIds == null || serviceIds.Count == 0)
        //        return BadRequest(new { message = "Vui lòng chọn dịch vụ." });

        //    var result = await _numberFomularSvc.ConvertToShared(serviceIds);

        //    if (!result)
        //        return BadRequest(new { message = "Chuyển sang Shared thất bại." });

        //    return Ok(new { message = "Chuyển sang Shared thành công." });
        //}

        // POST: api/NumberFomular/Reset/1
        //[HttpPost("Reset/{formulaId}")]
        //public IActionResult ReSet(int formulaId)
        //{
        //    var result = _numberFomularSvc.ReSet(formulaId);

        //    return Ok(new { message = result });
        //}


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _numberFomularSvc.Delete(id);

            if (result == "Không tìm thấy công thức.")
                return NotFound(new { message = result });

            if (result != "Xóa công thức thành công.")
                return BadRequest(new { message = result });

            return Ok(new { message = result });
        }
    }
}