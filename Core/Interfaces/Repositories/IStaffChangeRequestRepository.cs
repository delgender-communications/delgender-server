using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IStaffChangeRequestRepository : IRepository<StaffChangeRequest>
    {
        Task<StaffChangeRequest?> GetByIdWithPeopleAsync(int id);
        Task<List<StaffChangeRequest>> GetForStaffAsync(int staffId, int take);
        Task<List<StaffChangeRequest>> GetByStatusAsync(ChangeRequestStatus? status, int take);
        Task<bool> HasPendingAsync(int staffId, ChangeRequestType type);
        Task SaveAsync();
    }
}
