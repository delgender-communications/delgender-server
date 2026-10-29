using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IDocumentRepository : IRepository<TeamDocument>
    {
        Task<List<TeamDocument>> GetAllWithStaffAsync();
        Task<TeamDocument?> GetByIdWithStaffAsync(int id);
        Task<List<TeamDocument>> GetByIdsAsync(IReadOnlyCollection<int> ids);
        Task DeleteAsync(TeamDocument document);
    }
}
