using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext db) : base(db) { }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
            await _db.RefreshTokens
                .Include(r => r.Staff)
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

        public async Task RevokeActiveForDeviceAsync(int deviceId)
        {
            var now = DateTime.UtcNow;
            var tokens = await _db.RefreshTokens
                .Where(r => r.TrustedDeviceId == deviceId && r.RevokedAt == null)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                return;
            }

            foreach (var token in tokens)
            {
                token.RevokedAt = now;
                token.UpdatedAt = now;
            }

            await _db.SaveChangesAsync();
        }
    }
}
