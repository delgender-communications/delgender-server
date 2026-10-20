using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OutreachRepository : Repository<OutreachCompany>, IOutreachRepository
    {
        public OutreachRepository(AppDbContext db) : base(db) { }

        public async Task<List<OutreachCompany>> GetPageAsync(int page, int pageSize, OutreachStatus? status)
        {
            return await Filtered(status)
                .AsNoTracking()
                .OrderByDescending(o => o.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> CountAsync(OutreachStatus? status) =>
            Filtered(status).CountAsync();

        public Task<OutreachCompany?> GetByIdWithDetailsAsync(int id) =>
            _db.OutreachCompanies
                .Include(o => o.AddedByStaff)
                .Include(o => o.Logs)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        private IQueryable<OutreachCompany> Filtered(OutreachStatus? status)
        {
            var query = _db.OutreachCompanies.AsQueryable();
            if (status is not null)
            {
                query = query.Where(o => o.Status == status);
            }
            return query;
        }
    }
}
