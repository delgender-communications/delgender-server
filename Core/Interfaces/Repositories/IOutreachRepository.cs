using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IOutreachRepository : IRepository<OutreachCompany>
    {
        Task<List<OutreachCompany>> GetPageAsync(int page, int pageSize, OutreachStatus? status);
        Task<int> CountAsync(OutreachStatus? status);
        Task<OutreachCompany?> GetByIdWithDetailsAsync(int id);
        Task SaveChangesAsync();
    }
}
