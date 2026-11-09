using SystemQMS.ViewModels.NewsDto;

namespace SystemQMS.Services.Interfaces
{
    public interface INewsSvc
    {
        Task<(IEnumerable<NewsDto> Data, int TotalCount)> GetAllAsync(int unitId, string status, int page, int pageSize);
        Task<NewsDto> GetByIdAsync(int id, int unitId);
        Task<NewsDto> CreateAsync(NewsCreateDto request, int unitId, int userId);
        Task<bool> UpdateAsync(int id, NewsUpdateDto request, int unitId, int userId);
        Task<bool> DeleteAsync(int id, int unitId);
        Task<bool> BulkDeleteAsync(List<int> ids, int unitId);
    }
}