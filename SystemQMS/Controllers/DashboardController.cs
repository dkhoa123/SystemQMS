using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services;
using SystemQMS.Services.Interface;
using static SystemQMS.ViewModels.DashboardDto.DashboardDto;

namespace SystemQMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private IDashboardSvc dashboardSvc;
        public DashboardController(IDashboardSvc dashboardSvc)
        {
            this.dashboardSvc = dashboardSvc;
        }

        // unitId luôn lấy từ token (SRS 6.2: phạm vi dữ liệu theo Phường kiểm tra ở tầng API).
        // Claim "unitId" phải được phát hành lúc đăng nhập (ACC-01)
        // hiện chưa có đăng nhập nên lấy unitId = 1 để test.
        private bool TryGetUnitId(out int unitId)
        {
            var raw = User.FindFirst("unitId")?.Value;

            if (int.TryParse(raw, out unitId))
                return true;

            // Tạm thời dùng unitId = 1 để test
            unitId = 1;
            return true;
        }

        private static string? ValidateFilter(DashboardFilterRequest f)
        {
            if (f.From.HasValue && f.To.HasValue && f.From > f.To)
                return "Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc.";
            return null;
        }

        // ---------------- DSH-01 ----------------
        [HttpGet("tickets/issued")]
        public async Task<ActionResult<TicketSummaryDto>> TicketSummary(
            [FromQuery] string filter,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate
            )
        {
            try
            {
                // Lấy unitId từ tài khoản đang đăng nhập
                if (!TryGetUnitId(out var unitId))
                {
                    return Unauthorized(
                        "Không xác định được đơn vị của tài khoản."
                    );
                }

                // Chuyển string thành enum
                if (!Enum.TryParse<StatisticsFilter>(
                    filter,
                    true,
                    out var parsedFilter))
                {
                    return BadRequest(
                        "Filter phải là Gio, Ngay, Tuan hoặc Thang."
                    );
                }

                var result =
                    await dashboardSvc.GetTicketIssuedAsync(
                        unitId,
                        parsedFilter,
                        fromDate,
                        toDate
                    );

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    "Lỗi thống kê: " + ex.Message
                );
            }
        }

        // ---------------- DSH-02 ----------------
        [HttpGet("tickets/status")]
        public async Task<ActionResult<TicketStatusDonutDto>> TicketStatus()
        {
            return Ok(await dashboardSvc.GetTicketStatusAsync());
        }

        // ---------------- DSH-04 ----------------
        [HttpGet("counters-services/summary")]
        public async Task<ActionResult<CounterServiceSummaryDto>> CounterServiceSummary()
        {

            return Ok(await dashboardSvc.GetCounterServiceSummaryAsync());
        }

        // ---------------- DSH-06 ----------------
        [HttpGet("tickets/by-service")]
        public async Task<IActionResult> GetTicketsByService(
                [FromQuery] string filter,
                [FromQuery] DateTime? fromDate)
        {
            try
            {

                // Chuyển string thành enum
                if (!Enum.TryParse<StatisticsFilter>(
                    filter,
                    true,
                    out var parsedFilter))
                {
                    return BadRequest(
                        "Filter phải là Gio, Ngay, Tuan hoặc Thang."
                    );
                }
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                        OperatingSystem.IsWindows()
                        ? "SE Asia Standard Time"
                        : "Asia/Ho_Chi_Minh"
);

                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    timeZone
                );
                var date = fromDate ?? currentTime;

                var result = await dashboardSvc.GetTicketsByServiceAsync(
                    //unitId,
                    parsedFilter,
                    date
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ---------------- DSH-03 ----------------
        [HttpGet("tickets/trend")]
        public async Task<ActionResult> TicketTrend()
        {
            return Ok(await dashboardSvc.GetTicketTrendAsync());
        }

        [HttpGet("services/statistics")]
        public async Task<IActionResult> GetServiceStatistics()
        {
            try
            {
                var result = await dashboardSvc.GetServiceStatisticsAsync(
                    //unitId.Value,
                );

                return Ok(new
                {
                    success = true,
                    data = result
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
    }
}
