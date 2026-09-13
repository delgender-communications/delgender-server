using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface ITrustedDeviceRepository : IRepository<TrustedDevice>
    {
        Task<TrustedDevice?> GetByTokenHashAsync(string tokenHash);
        Task<IEnumerable<TrustedDevice>> GetActiveForStaffAsync(int staffId);
    }
}
