using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using SystemQMS.Models;
using SystemQMS.Services;
using ClosedXML;
using System.IO;
using ClosedXML.Excel;
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
        public IActionResult AddNew([FromBody] Sticker sticker)
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
    }
}