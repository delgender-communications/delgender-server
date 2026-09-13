using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StaffRepository : Repository<Staff>, IStaffRepository
    {
        public StaffRepository(AppDbContext db) : base(db) { }

        public async Task<Staff?> GetByEmailAsync(string email) =>
            await _db.Staffs.FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());

        public async Task<int> GetCountAsync() =>
            await _db.Staffs.CountAsync();

        public async Task<string> GenerateNextStaffCodeAsync()
        {
            var count = await _db.Staffs.CountAsync();
            return $"DGC-{(count + 1):D4}";
        }
    }
}
