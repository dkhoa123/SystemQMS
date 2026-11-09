using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interfaces;
using SystemQMS.ViewModels.NewsDto;

namespace SystemQMS.Services
{
    public class NewsSvc : INewsSvc
    {
        private readonly MyDbContext _context;

        public NewsSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<NewsDto> Data, int TotalCount)> GetAllAsync(int unitId, string status, int page, int pageSize)
        {
            var query = _context.News
                .Include(n => n.CreatedBy)
                .Where(n => n.UnitId == unitId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.Status == status);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(n => n.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NewsDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    AttachmentUrl = n.AttachmentUrl,
                    PublishDate = n.PublishDate,
                    Status = n.Status,
                    CreatedByName = n.CreatedBy.username
                })
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<NewsDto> GetByIdAsync(int id, int unitId)
        {
            var news = await _context.News
                .Include(n => n.CreatedBy)
                .FirstOrDefaultAsync(n => n.Id == id && n.UnitId == unitId);

            if (news == null) return null;

            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                AttachmentUrl = news.AttachmentUrl,
                PublishDate = news.PublishDate,
                Status = news.Status,
                CreatedByName = news.CreatedBy.username
            };
        }

        public async Task<NewsDto> CreateAsync(NewsCreateDto request, int unitId, int userId)
        {
            var news = new News
            {
                Title = request.Title,
                Content = request.Content,
                AttachmentUrl = request.AttachmentUrl,
                Status = request.Status,
                PublishDate = DateTimeOffset.UtcNow,
                UnitId = unitId,
                CreatedById = userId
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(news.Id, unitId);
        }

        public async Task<bool> UpdateAsync(int id, NewsUpdateDto request, int unitId, int userId)
        {
            var news = await _context.News.FirstOrDefaultAsync(n => n.Id == id && n.UnitId == unitId);
            if (news == null) return false;

            news.Title = request.Title;
            news.Content = request.Content;
            news.AttachmentUrl = request.AttachmentUrl;
            news.Status = request.Status;

            news.UpdatedById = userId;
            if (request.Status == "Đã xuất bản" && news.Status != "Đã xuất bản")
            {
                news.PublishDate = DateTimeOffset.UtcNow;
            }

            _context.News.Update(news);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int unitId)
        {
            var news = await _context.News.FirstOrDefaultAsync(n => n.Id == id && n.UnitId == unitId);
            if (news == null) return false;

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteAsync(List<int> ids, int unitId)
        {
            var newsList = await _context.News
                .Where(n => ids.Contains(n.Id) && n.UnitId == unitId)
                .ToListAsync();

            if (!newsList.Any()) return false;

            _context.News.RemoveRange(newsList);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}