using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface ILoginOtpRepository : IRepository<LoginOtp>
    {
        Task<LoginOtp?> GetLatestActiveForStaffAsync(int staffId);
        Task<LoginOtp?> GetLatestForStaffAsync(int staffId);
    }
}
