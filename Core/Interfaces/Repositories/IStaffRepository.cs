using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IStaffRepository : IRepository<Staff>
    {
        Task<Staff?> GetByEmailAsync(string email);
        Task<int> GetCountAsync();
        Task<string> GenerateNextStaffCodeAsync();
    }
}
