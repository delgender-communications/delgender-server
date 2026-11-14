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
                .OrderBy(o =>
                o.Status == OutreachStatus.Ready ? 0 :
                o.Status == OutreachStatus.Researching ? 1 :
                o.Status == OutreachStatus.Emailed ? 2 : 3)
                .ThenByDescending(o => o.UpdatedAt)
                .ThenByDescending(o => o.Id)
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
            .Include(o => o.NoteEditors)
            .ThenInclude(e => e.Staff)
            .FirstOrDefaultAsync(o => o.Id == id);

        public Task<ResearchLead?> GetSourceLeadAsync(int outreachCompanyId) =>
            _db.ResearchLeads
            .AsNoTracking()
            .Include(l => l.Evidence)
            .Include(l => l.AddedByStaff)
            .FirstOrDefaultAsync(l => l.OutreachCompanyId == outreachCompanyId);

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
