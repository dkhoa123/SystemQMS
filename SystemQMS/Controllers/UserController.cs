using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.UserDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserSvc _userSvc;
        public UserController(IUserSvc userSvc)
        {
            _userSvc = userSvc;
        }

        [HttpGet("/get/{id}")]
        public IActionResult GetUser(int id)
        {
            try
            {
                var user = _userSvc.GetUser(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("/update/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] Users user)
        {
            try
            {
                var updatedUser = _userSvc.UpdateUser(id, user);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("/getAll")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userSvc.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpGet("/delete/{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var result = _userSvc.DeleteUser(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Thêm endpoint cho quản lý tài khoản 

        [HttpGet("/api/v1/users")]
        public async Task<IActionResult> GetPagedUsers([FromQuery] UserQueryDto query)
        {
            var result = await _userSvc.GetPagedUsersAsync(query);
            return Ok(result);
        }

        [HttpGet("/api/v1/users/{id:int}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            try
            {
                var result = await _userSvc.GetUserDetailAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("/api/v1/users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var created = await _userSvc.CreateUserAsync(dto);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("/api/v1/users/{id:int}")]
        public async Task<IActionResult> UpdateUserAccount(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var updated = await _userSvc.UpdateUserAccountAsync(id, dto);
                return Ok(updated);
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

        [HttpPut("/api/v1/users/{id:int}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangeUserPasswordDto dto)
        {
            try
            {
                await _userSvc.ChangePasswordAsync(id, dto);
                return Ok(new { message = "Đổi mật khẩu thành công." });
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

        [HttpDelete("/api/v1/users")]
        public async Task<IActionResult> DeleteUsers([FromBody] List<int> ids)
        {
            try
            {
                await _userSvc.DeleteUsersAsync(ids);
                return Ok(new { message = "Xóa tài khoản thành công." });
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

    }
}
