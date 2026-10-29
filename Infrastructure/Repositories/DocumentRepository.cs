using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DocumentRepository : Repository<TeamDocument>, IDocumentRepository
    {
        public DocumentRepository(AppDbContext db) : base(db) { }

        public Task<List<TeamDocument>> GetAllWithStaffAsync() =>
            _db.TeamDocuments
                .AsNoTracking()
                .Include(d => d.UploadedByStaff)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

        public Task<TeamDocument?> GetByIdWithStaffAsync(int id) =>
            _db.TeamDocuments
                .Include(d => d.UploadedByStaff)
                .FirstOrDefaultAsync(d => d.Id == id);

        public Task<List<TeamDocument>> GetByIdsAsync(IReadOnlyCollection<int> ids) =>
            _db.TeamDocuments
                .Where(d => ids.Contains(d.Id))
                .ToListAsync();

        public async Task DeleteAsync(TeamDocument document)
        {
            _db.TeamDocuments.Remove(document);
            await _db.SaveChangesAsync();
        }
    }
}
