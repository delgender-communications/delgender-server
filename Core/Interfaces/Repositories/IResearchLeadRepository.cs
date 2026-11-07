using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IResearchLeadRepository : IRepository<ResearchLead>
    {
        Task<List<ResearchLead>> GetPageAsync(int page, int pageSize, ResearchLeadStatus? status, ResearchLeadCase? leadCase, bool? needsResearch = null);
        Task<int> CountAsync(ResearchLeadStatus? status, ResearchLeadCase? leadCase, bool? needsResearch = null);
        Task<int> CountNewAsync(bool? needsResearch = null);
        Task<ResearchLead?> GetByIdWithDetailsAsync(int id);

        Task<List<ResearchLead>> FindExistingAsync(IReadOnlyCollection<string> hosts, IReadOnlyCollection<string> nameKeys);

        Task<List<string>> GetOutreachCompanyNamesAsync();
        Task AddRangeAsync(IEnumerable<ResearchLead> leads);
        Task SaveChangesAsync();
    }
}
