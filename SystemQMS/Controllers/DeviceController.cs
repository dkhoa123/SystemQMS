using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.DevicesDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private IDevSvc _devService;
        public DeviceController(IDevSvc devService)
        {
            _devService = devService;
        }

        [HttpPost("create")]
        public IActionResult CreateDevice([FromBody] CreateDeviceDto device, [FromQuery] int unitId)
        {
            try
            {
                _devService.createDevice(device, unitId);
                return Ok(device);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                throw new Exception($"Error creating device: {ex.Message}. Chi tiết lỗi (Inner): {innerMessage}");
            }
        }
        [HttpPut("updateStatus/{deviceId}")]
        public IActionResult UpdateDeviceStatus(string deviceId)
        {
            try
            {
                _devService.updateDeviceStatus(deviceId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("update/{deviceId}")]
        public IActionResult UpdateDevice(int deviceId, [FromBody] UpdateDeviceDto device)
        {
            try
            {
                _devService.updateDevice(deviceId, device);
                return Ok(device);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get/{deviceId}")]
        public IActionResult GetDevice(int deviceId)
        {
            try
            {
                var device = _devService.GetDevices(deviceId);
                return Ok(device);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAll")]
        public IActionResult GetAllDevices([FromQuery] FilterDeviceDto filterDevice)
        {
            try
            {
                var devices = _devService.GetAllDevices(filterDevice);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generate-code/{deviceId}")]
        public async Task<IActionResult> GenerateCode(string deviceId)
        {
            try
            {
                var code = await _devService.GenerateLoginCode(deviceId);

                return Ok(new
                {
                    message = "Tạo mã đăng nhập thành công",
                    code
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // CMS nhập mã để đăng nhập thiết bị
        [HttpPost("login")]
        public async Task<IActionResult> Login(DeviceLoginDto dto)
        {
            try
            {
                var result = await _devService.LoginCode(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("delete")]
        public IActionResult DeleteDevice([FromBody] DeleteDeviceDto devices)
        {
            try
            {
                var result = _devService.DeleteDevice(devices);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-volume")]
        public IActionResult UpdateVolumeDevices([FromBody] UpdateVolumnDeviceDto updateVolumnDeviceDto)
        {
            try
            {
                var result = _devService.UpdateVolumeDevices(updateVolumnDeviceDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
