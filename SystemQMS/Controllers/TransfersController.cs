using Microsoft.AspNetCore.Mvc;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.TransfersDto;

namespace SystemQMS.Controllers
{
    [Route("api/transfers")]
    [ApiController]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferSvc _transferSvc;

        public TransfersController(ITransferSvc transferSvc)
        {
            _transferSvc = transferSvc;
        }

        // OPR-07: quầy gửi tạo yêu cầu chuyển phiếu
        [HttpPost("/api/counters/{counterId}/transfer")]
        public async Task<ActionResult<List<TransferResponse>>> CreateTransfer(
            int counterId, TransferRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _transferSvc.CreateTransferAsync(counterId, request, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // OPR-07: danh sách yêu cầu đang chờ quầy này tiếp nhận
        [HttpGet("/api/counters/{counterId}/transfers/pending")]
        public async Task<ActionResult<List<TransferResponse>>> GetPending(int counterId, CancellationToken ct)
        {
            try
            {
                return Ok(await _transferSvc.GetPendingTransfersAsync(counterId, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // OPR-07: quầy nhận phản hồi
        [HttpPost("{id}/action")]
        public async Task<ActionResult<TransferResponse>> Action(
            int id, TransferActionRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _transferSvc.ActionAsync(id, request.action, ct));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
