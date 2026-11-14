using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate
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

        // ---------------- DSH-03 ----------------
        [HttpGet("tickets/trend")]
        public async Task<ActionResult<ChartSeriesDto>> TicketTrend(
            [FromQuery] DashboardFilterRequest f, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            if (ValidateFilter(f) is { } err) return BadRequest(new { message = err });
            return Ok(await dashboardSvc.GetTicketTrendAsync(unitId, f, ct));
        }

        // ---------------- DSH-04 ----------------
        [HttpGet("counters-services/summary")]
        public async Task<ActionResult<CounterServiceSummaryDto>> CounterServiceSummary()
        {
            
            return Ok(await dashboardSvc.GetCounterServiceSummaryAsync());
        }

        // ---------------- DSH-05 ----------------
        [HttpGet("ratings/services")]
        public async Task<ActionResult<ServiceRatingDashboardDto>> ServiceRatings(
            [FromQuery] DashboardFilterRequest f, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            if (ValidateFilter(f) is { } err) return BadRequest(new { message = err });
            return Ok(await dashboardSvc.GetServiceRatingsAsync(unitId, f, ct));
        }

        // ---------------- DSH-06 ----------------
        [HttpGet("tickets/by-service")]
        public async Task<IActionResult> GetTicketsByService(
                [FromQuery] string filter,
                [FromQuery] DateTime fromDate)
        {
            try
            {
                int unitId = 1;

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

                var result = await dashboardSvc.GetTicketsByServiceAsync(
                    unitId,
                    parsedFilter,
                    fromDate
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tickets/by-counter")]
        public async Task<ActionResult<ChartSeriesDto>> TicketsByCounter(
            [FromQuery] DashboardFilterRequest f, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            if (ValidateFilter(f) is { } err) return BadRequest(new { message = err });
            return Ok(await dashboardSvc.GetTicketsByCounterAsync(unitId, f, ct));
        }

        // ---------------- DSH-07 ----------------
        [HttpGet("wait-time/services")]
        public async Task<ActionResult<ChartSeriesDto>> WaitTimeByService(
            [FromQuery] DashboardFilterRequest f, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            if (ValidateFilter(f) is { } err) return BadRequest(new { message = err });
            return Ok(await dashboardSvc.GetWaitTimeByServiceAsync(unitId, f, ct));
        }

        // ---------------- DSH-08 ----------------
        [HttpGet("process-time/counters")]
        public async Task<ActionResult<ChartSeriesDto>> ProcessTimeByCounter(
            [FromQuery] DashboardFilterRequest f, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            if (ValidateFilter(f) is { } err) return BadRequest(new { message = err });
            return Ok(await dashboardSvc.GetProcessTimeByCounterAsync(unitId, f, ct));
        }

        // ---------------- DSH-09 ----------------
        [HttpGet("peak-hours")]
        public async Task<ActionResult<HeatmapDto>> PeakHours(
            [FromQuery] HeatmapRequest r, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            return Ok(await dashboardSvc.GetPeakHoursAsync(unitId, r, ct));
        }

        // ---------------- DSH-10 / 11 / 12 ----------------
        [HttpGet("devices/summary")]
        public async Task<ActionResult<DeviceSummaryDto>> DeviceSummary(CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            return Ok(await dashboardSvc.GetDeviceSummaryAsync(unitId, ct));
        }

        // DSH-11: gọi trước khi đổi thiết bị sang "Không khả dụng" để quyết định hiện popup cảnh báo.
        // Dùng "~/" để route nằm ngoài tiền tố api/dashboard, khớp với /api/devices/{id}/deactivate-check.
        [HttpGet("~/api/devices/{id:int}/deactivate-check")]
        public async Task<ActionResult<DeviceDeactivateCheckDto>> DeviceDeactivateCheck(
            int id, CancellationToken ct)
        {
            if (!TryGetUnitId(out var unitId)) return Unauthorized();
            var result = await dashboardSvc.CheckDeviceDeactivateAsync(unitId, id, ct);
            return result is null ? NotFound(new { message = "Không tìm thấy thiết bị." }) : Ok(result);
        }
    }
}
