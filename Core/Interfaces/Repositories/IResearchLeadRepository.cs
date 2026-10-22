using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IResearchLeadRepository : IRepository<ResearchLead>
    {
        Task<List<ResearchLead>> GetPageAsync(int page, int pageSize, ResearchLeadStatus? status, ResearchLeadCase? leadCase);
        Task<int> CountAsync(ResearchLeadStatus? status, ResearchLeadCase? leadCase);
        Task<int> CountNewAsync();
        Task<ResearchLead?> GetByIdWithDetailsAsync(int id);

        // Leads that share a website host or a name key with any of the given ones.
        Task<List<ResearchLead>> FindExistingAsync(IReadOnlyCollection<string> hosts, IReadOnlyCollection<string> nameKeys);

        Task<List<string>> GetOutreachCompanyNamesAsync();
        Task AddRangeAsync(IEnumerable<ResearchLead> leads);
        Task SaveChangesAsync();
    }
}
