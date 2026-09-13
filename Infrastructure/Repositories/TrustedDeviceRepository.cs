using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TrustedDeviceRepository : Repository<TrustedDevice>, ITrustedDeviceRepository
    {
        public TrustedDeviceRepository(AppDbContext db) : base(db) { }

        public async Task<TrustedDevice?> GetByTokenHashAsync(string tokenHash) =>
            await _db.TrustedDevices
                .Include(t => t.Staff)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        public async Task<IEnumerable<TrustedDevice>> GetActiveForStaffAsync(int staffId) =>
            await _db.TrustedDevices
                .Where(t => t.StaffId == staffId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.LastUsedAt)
                .ToListAsync();
    }
}
