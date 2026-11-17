using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using SystemQMS.Models;
using ClosedXML;
using System.IO;
using ClosedXML.Excel;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.TicketDto;
namespace SystemQMS.Controllers
{
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
        public IActionResult AddNew([FromBody] Tickets sticker)
        {
            if (sticker == null)
                return BadRequest("Dữ liệu phiếu không hợp lệ");
            var result = _sticker.AddNew(sticker);
            return Ok(result);
        }

        [HttpGet("export")]
        
        public IActionResult Export()
        {
            var file = _sticker.ExportExcel();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachSticker.xlsx"
            );
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var statistics = _sticker.GetStatistics();
            return Ok(statistics);
        }

        [HttpGet("tickets")]
        public IActionResult GetTickets([FromQuery] FilterTicketDto filter)
        {
            Console.WriteLine("========== GET TICKETS API ==========");
            Console.WriteLine($"Page: {filter.page}");
            Console.WriteLine($"PageSize: {filter.pageSize}");

            var tickets = _sticker.GetTickets(filter);

            Console.WriteLine("========== GET TICKETS SUCCESS ==========");

            return Ok(tickets);
        }

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            var unitId = 1;

            if (unitId == null)
                return Unauthorized();

            try
            {
                var result = await _sticker
                    .GetRulesAsync(unitId);

                return Ok(new
                {
                    success = true,
                    data = result
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

        [HttpPut("rules/range")]
        public async Task<IActionResult> UpdateRange(
    [FromBody] UpdateTicketRangeDto dto)
        {
            var unitId = 1;

            if (unitId == null)
                return Unauthorized();

            try
            {
                var result = await _sticker
                    .UpdateRangeAsync(unitId, dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Đơn vị chưa có quy tắc cấp số."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật dải số thành công."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("create/tickets")]
        public async Task<IActionResult> IssueTicket(
    [FromBody] IssueTicketRequest request)
        {
            var unitId = 1;
            var userId = 1;

            if (unitId == null || userId == null)
                return Unauthorized();

            try
            {
                var result = await _sticker
                    .IssueTicketAsync(
                        unitId,
                        userId,
                        request
                    );

                return Ok(new
                {
                    success = true,
                    message = "Cấp số thành công.",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
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
    }
}