using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.RoleDto;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleSvc _roleSvc;

        public RolesController(IRoleSvc roleSvc)
        {
            _roleSvc = roleSvc;
        }

        [HttpGet("permissions")]
        public IActionResult GetPermissionGroups()
        {
            var data = _roleSvc.GetPermissionGroups();
            return Ok(new { data });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetList([FromQuery] RoleQueryFilterDto filter)
        {
            var data = await _roleSvc.GetRolesAsync(filter);
            return Ok(new { data });
        }

        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var data = await _roleSvc.GetRoleByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "Không tìm thấy vai trò" });

            return Ok(new { data });
        }

        [HttpPost("roles")]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var data = await _roleSvc.CreateRoleAsync(request);
                return Ok(new { data });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var data = await _roleSvc.UpdateRoleAsync(id, request);
                return Ok(new { data });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("roles")]
        public async Task<IActionResult> Remove([FromBody] DeleteRolesRequestDto request)
        {
            try
            {
                await _roleSvc.DeleteRolesAsync(request.ids);
                return Ok(new { message = "Đã xóa vai trò" });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}