using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StaffChangeRequestRepository : Repository<StaffChangeRequest>, IStaffChangeRequestRepository
    {
        public StaffChangeRequestRepository(AppDbContext db) : base(db) { }

        public Task<StaffChangeRequest?> GetByIdWithPeopleAsync(int id) =>
            _db.StaffChangeRequests
                .Include(r => r.Staff)
                .Include(r => r.ReviewedByStaff)
                .FirstOrDefaultAsync(r => r.Id == id);

        public Task<List<StaffChangeRequest>> GetForStaffAsync(int staffId, int take) =>
            _db.StaffChangeRequests
                .AsNoTracking()
                .Include(r => r.Staff)
                .Include(r => r.ReviewedByStaff)
                .Where(r => r.StaffId == staffId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToListAsync();

        public Task<List<StaffChangeRequest>> GetByStatusAsync(ChangeRequestStatus? status, int take)
        {
            var query = _db.StaffChangeRequests
                .AsNoTracking()
                .Include(r => r.Staff)
                .Include(r => r.ReviewedByStaff)
                .AsQueryable();

            if (status is not null)
            {
                query = query.Where(r => r.Status == status);
            }

            return query.OrderByDescending(r => r.CreatedAt).Take(take).ToListAsync();
        }

        public Task<bool> HasPendingAsync(int staffId, ChangeRequestType type) =>
            _db.StaffChangeRequests.AnyAsync(r =>
                r.StaffId == staffId && r.Type == type && r.Status == ChangeRequestStatus.Pending);

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
