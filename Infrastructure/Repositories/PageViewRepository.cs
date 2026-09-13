using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PageViewRepository : Repository<PageView>, IPageViewRepository
    {
        public PageViewRepository(AppDbContext db) : base(db) { }

        public async Task<List<PageView>> GetForDateAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            return await _db.PageViews
                .Where(p => p.VisitedAt >= start && p.VisitedAt < end)
                .ToListAsync();
        }

        public async Task<List<PageView>> GetPagedAsync(int page, int pageSize) =>
            await _db.PageViews
                .OrderByDescending(p => p.VisitedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync() =>
            await _db.PageViews.CountAsync();
    }
}
