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
    }
}
