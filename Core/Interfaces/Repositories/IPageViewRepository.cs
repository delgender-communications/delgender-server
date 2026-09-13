using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IPageViewRepository : IRepository<PageView>
    {
        Task<List<PageView>> GetForDateAsync(DateOnly date);
        Task<List<PageView>> GetPagedAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
    }
}
