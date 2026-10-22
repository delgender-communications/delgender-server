using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ResearchLeadRepository : Repository<ResearchLead>, IResearchLeadRepository
    {
        public ResearchLeadRepository(AppDbContext db) : base(db) { }

        public async Task<List<ResearchLead>> GetPageAsync(
            int page, int pageSize, ResearchLeadStatus? status, ResearchLeadCase? leadCase)
        {
            return await Filtered(status, leadCase)
                .AsNoTracking()
                .Include(l => l.Evidence)
                .Include(l => l.AddedByStaff)
                .Include(l => l.ReviewedByStaff)
                .OrderByDescending(l => l.Score ?? -1)
                .ThenByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> CountAsync(ResearchLeadStatus? status, ResearchLeadCase? leadCase) =>
            Filtered(status, leadCase).CountAsync();

        public Task<int> CountNewAsync() =>
            _db.ResearchLeads.CountAsync(l => l.Status == ResearchLeadStatus.New);

        public Task<ResearchLead?> GetByIdWithDetailsAsync(int id) =>
            _db.ResearchLeads
                .Include(l => l.Evidence)
                .Include(l => l.AddedByStaff)
                .Include(l => l.ReviewedByStaff)
                .Include(l => l.OutreachCompany)
                .FirstOrDefaultAsync(l => l.Id == id);

        public Task<List<ResearchLead>> FindExistingAsync(
            IReadOnlyCollection<string> hosts, IReadOnlyCollection<string> nameKeys) =>
            _db.ResearchLeads
                .AsNoTracking()
                .Where(l => nameKeys.Contains(l.NameKey) ||
                            (l.WebsiteHost != null && hosts.Contains(l.WebsiteHost)))
                .ToListAsync();

        public Task<List<string>> GetOutreachCompanyNamesAsync() =>
            _db.OutreachCompanies.AsNoTracking().Select(o => o.CompanyName).ToListAsync();

        public async Task AddRangeAsync(IEnumerable<ResearchLead> leads)
        {
            _db.ResearchLeads.AddRange(leads);
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        private IQueryable<ResearchLead> Filtered(ResearchLeadStatus? status, ResearchLeadCase? leadCase)
        {
            var query = _db.ResearchLeads.AsQueryable();

            if (status is not null)
            {
                query = query.Where(l => l.Status == status);
            }

            if (leadCase is not null)
            {
                query = query.Where(l => l.Case == leadCase);
            }

            return query;
        }
    }
}
