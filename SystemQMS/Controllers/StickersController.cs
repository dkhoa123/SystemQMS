using ClosedXML;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net.WebSockets;
using SystemQMS.Models;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CallNumberDto;
using SystemQMS.ViewModels.CallNumberDto.SystemQMS.DTOs;
using SystemQMS.ViewModels.Sticker;
namespace SystemQMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StickersController : ControllerBase
{
    private IStickerSvc _sticker;
    public StickersController(IStickerSvc sticker)
    {
        _sticker = sticker;
    }

    [HttpPost]
    public async Task<IActionResult> AddNew([FromBody] CreateStickerDto dto)
    {
        if (dto == null)
            return BadRequest("Dữ liệu phiếu không hợp lệ");
        var result = await _sticker.AddNew(dto);
        return Ok(result);
    }

    [HttpGet("export-excel")]
    
    public IActionResult Export()
    {
        var file = _sticker.ExportExcel();

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "DanhSachSticker.xlsx"
        );
    }

    [HttpPost("ResetNumbers")]
    public async Task<IActionResult> ResetNumbers()
    {
        try
        {
            var count = await _sticker.ResetNumbers();

            return Ok(new
            {
                message = "Reset dãy số thành công.",
                cancelledCount = count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    [HttpGet("list")]
    public async Task<IActionResult> GetStickerList()
    {
        var data = await _sticker.GetStickerList();

        return Ok(data);
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext([FromBody] CallNumberRequest request)
    {
        var sticker = await _sticker.CallNext(request.CounterId);

        if (sticker == null)
            return NotFound("Không còn số trong hàng chờ.");

        return Ok(sticker);
    }

    [HttpPost("call-multiple")]
    public async Task<IActionResult> CallMultiple([FromBody] CallMultipleRequest request)
    {
        if (request.Quantity < 1 || request.Quantity > 6)
            return BadRequest("Số lượng gọi phải từ 1 đến 6.");

        var stickers = await _sticker.CallMultiple(
            request.CounterId,
            request.Quantity);

        if (stickers.Count == 0)
            return NotFound("Không còn số trong hàng chờ.");

        return Ok(stickers);
    }

    [HttpPost("call-again")]
    public async Task<IActionResult> CallAgain(
    [FromBody] CallAgainRequestDto request)
    {
        var sticker = await _sticker.CallAgain(request.CounterId);

        if (sticker == null)
        {
            return NotFound("Quầy hiện không có STT để gọi lại.");
        }

        return Ok(sticker);
    }
    [HttpPost("skip")]
    public async Task<IActionResult> Skip(
    [FromBody] SkipRequestDto request)
    {
        var sticker = await _sticker.Skip(request.CounterId);

        if (sticker == null)
        {
            return NotFound("Quầy hiện không có STT để bỏ qua.");
        }

        return Ok(sticker);
    }
    [HttpPost("transfer-counter")]
    public async Task<IActionResult> TransferCounter(
    [FromBody] TransferCounterRequest request)
    {
        try
        {
            var result = await _sticker.TransferCounter(
                request.SttId,
                request.TargetCounterId);

            if (result == null)
                return NotFound("Không tìm thấy STT.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

        [HttpPost("transfer-selected")]
        public async Task<IActionResult> TransferSelected(
        [FromBody] BulkTransferCounterRequest request)
        {
            if (request.SttIds == null || !request.SttIds.Any())
                return BadRequest("Vui lòng chọn ít nhất một STT.");

            if (request.TargetCounterId <= 0)
                return BadRequest("Quầy đích không hợp lệ.");

            var result = await _sticker.TransferSelected(
                request.SttIds,
                request.TargetCounterId);

            return Ok(result);
        }
}