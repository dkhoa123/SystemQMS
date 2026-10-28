using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.AudioDto;

[ApiController]
[Route("api/v1/configs/display")]
public class DisplayConfigController : ControllerBase
{
    private readonly IConfigSvc _configService;

    public DisplayConfigController(IConfigSvc configService)
    {
        _configService = configService;
    }

    private int GetCurrentUnitId() => 1;

    [HttpGet]
    public async Task<IActionResult> GetDisplayConfig()
    {
        var config = await _configService.GetDisplayConfigAsync(GetCurrentUnitId());
        if (config == null) return NotFound("Chưa có cấu hình cho đơn vị này.");
        return Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDisplayConfig([FromForm] DisplayConfigUpdateDto dto)
    {
        var updatedConfig = await _configService.UpdateDisplayConfigAsync(GetCurrentUnitId(), dto);
        return Ok(new { message = "Cập nhật thành công", data = updatedConfig });
    }

    [HttpDelete("{fieldName}")]
    public async Task<IActionResult> ResetDisplayField(string fieldName)
    {
        await _configService.ResetDisplayFieldAsync(GetCurrentUnitId(), fieldName);
        return Ok(new { message = $"Đã reset trường {fieldName} về mặc định." });
    }

    [HttpGet("colors")]
    public IActionResult GetColorPalette()
    {
        var colors = new[] {
            new { Name = "Xanh dương nhạt", Hex = "#ADD8E6" },
            new { Name = "Xanh dương đậm", Hex = "#0057B8" },
            new { Name = "Đỏ", Hex = "#FF3B30" },
            new { Name = "Cam", Hex = "#FFA500" },
            new { Name = "Trắng", Hex = "#FFFFFF" },
            new { Name = "Đen", Hex = "#000000" },
            new { Name = "Xanh lá nhạt", Hex = "#90EE90" },
            new { Name = "Xanh lá đậm", Hex = "#006400" },
            new { Name = "Vàng", Hex = "#FFD700" },
            new { Name = "Tím", Hex = "#800080" }
        };
        return Ok(colors);
    }
}

[ApiController]
[Route("api/v1/configs/audio")]
public class AudioConfigController : ControllerBase
{
    private readonly IConfigSvc _configService;
    private readonly IAudioMappingSvc _audioService;

    public AudioConfigController(IConfigSvc configService, IAudioMappingSvc audioService)
    {
        _configService = configService;
        _audioService = audioService;
    }

    private int GetCurrentUnitId() => 1;

    [HttpPut]
    public async Task<IActionResult> UpdateAudioConfig([FromBody] AudioConfigUpdateDto dto)
    {
        await _configService.UpdateAudioConfigAsync(GetCurrentUnitId(), dto.TemplateId);
        return Ok(new { message = "Cập nhật mẫu câu đọc thành công" });
    }

    [HttpPost("preview")]
    public IActionResult PreviewAudio([FromBody] AudioPreviewDto request)
    {
        var audioUrls = _audioService.BuildAudioSequence(request.SampleTicketNumber, request.SampleCounterName, request.TemplateId);
        return Ok(new { playSequence = audioUrls });
    }

    [HttpGet("AudioTemplates")]
    public IActionResult GetAudioTemplates()
    {
        var templates = new List<AudioTemplateDto>
        {
            new AudioTemplateDto
            {
                Id = 1,
                Name = "Đọc đầy đủ",
                Description = "Xin mời số thứ tự [...] đến quầy [...]"
            },
            new AudioTemplateDto
            {
                Id = 2,
                Name = "Đọc ngắn gọn",
                Description = "[...] đến quầy [...]"
            },
            new AudioTemplateDto
            {
                Id = 3,
                Name = "Chỉ đọc số thứ tự",
                Description = "Xin mời số thứ tự [...]"
            }
        };

        return Ok(templates);
    }
}