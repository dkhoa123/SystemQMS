using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services;
using SystemQMS.ViewModels;

namespace SystemQMS.Controllers
{
    [ApiController]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null)
        {
            var result = await _roleService.GetRolesAsync(page, pageSize, keyword);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò." });
            }
            return Ok(role);
        }

        [HttpGet("permissions")]
        public IActionResult GetPermissionGroups()
        {
            var groups = _roleService.GetPermissionGroups();
            return Ok(groups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateUpdateRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _roleService.CreateRoleAsync(dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] CreateUpdateRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _roleService.UpdateRoleAsync(id, dto);
                if (updated == null)
                {
                    return NotFound(new { message = "Không tìm thấy vai trò." });
                }
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRoles([FromBody] List<string> ids)
        {
            try
            {
                var parsedIds = (ids ?? new List<string>())
                    .Select(id => int.TryParse(id, out var val) ? val : 0)
                    .Where(val => val > 0)
                    .ToList();

                await _roleService.DeleteRolesAsync(parsedIds);
                return Ok(new { message = "Xóa vai trò thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}