using SystemQMS.Models;
using SystemQMS.ViewModels.PageResultDto;
using SystemQMS.ViewModels.TicketDto;

namespace SystemQMS.Services.Interface
{
    public interface IStickerSvc
    {
        public string AddNew(Tickets sticker);
        byte[] ExportExcel();

        public TicketStatisticsDto GetStatistics();
        public PageResultDto<GetTicketDto> GetTickets(FilterTicketDto filter);
        Task<List<TicketRuleDto>> GetRulesAsync(
            int unitId
        );

        Task<TicketResponseDto> IssueTicketAsync(
            int unitId,
            int userId,
            IssueTicketRequest request
        );

        Task<bool> UpdateRangeAsync(
            int unitId,
            UpdateTicketRangeDto request
        );
    }
}
