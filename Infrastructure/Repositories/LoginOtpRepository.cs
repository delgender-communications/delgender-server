using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class LoginOtpRepository : Repository<LoginOtp>, ILoginOtpRepository
    {
        public LoginOtpRepository(AppDbContext db) : base(db) { }

        public async Task<LoginOtp?> GetLatestActiveForStaffAsync(int staffId) =>
            await _db.LoginOtps
                .Where(o => o.StaffId == staffId && o.ConsumedAt == null && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
    }
}
