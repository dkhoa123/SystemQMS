using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

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

    }
}
