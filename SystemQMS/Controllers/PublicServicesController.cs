using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.ViewModels.PublicService;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicServicesController : ControllerBase
    {
        private MyDbContext _context;
        public PublicServicesController(MyDbContext context)
        {
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> GetServices([FromQuery] string? search)
        {

            var query = _context.services.AsNoTracking().Where(s => s.isActive && s.allowOnlineBooking);

            // tim theo ten
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(s => s.serviceName.ToLower().Contains(keyword));
            }

            var services = await query
                .OrderBy(x => x.serviceName)
                .Select(x => new PublicServiceResponse
                {
                    id = x.id,
                    serviceId = x.serviceId,
                    serviceName = x.serviceName,
                    description = x.description,
                    image = x.image,
                    sharingLink = x.sharingLink,
                    averageTime = x.averageTime
                }
                )
                .ToListAsync();
            return Ok(new
            {
                message = services.Count > 0 ? "Lấy danh sách dịch vụ thành công" : "Không có dịch vụ nào",
                data = services,
                total = services.Count
            });



        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {

            var service = await _context.services.AsNoTracking()
                .Where(s => s.id == id && s.isActive && s.allowOnlineBooking)
                .Select(x => new PublicServiceResponse
                {
                    id = x.id,
                    serviceId = x.serviceId,
                    serviceName = x.serviceName,
                    description = x.description,
                    image = x.image,
                    sharingLink = x.sharingLink,
                    averageTime = x.averageTime
                })
                .FirstOrDefaultAsync();
            if (service == null)
            {
                return NotFound(new { message = "Dịch vụ không tồn tại" });
            }
            return Ok(new { message = "Lấy thông tin dịch vụ thành công", data = service });
        }

    }
}
