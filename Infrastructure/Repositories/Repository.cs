using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        public Repository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _db.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id) =>
            await _db.Set<T>().FindAsync(id);

        public async Task<T> CreateAsync(T entity)
        {
            _db.Set<T>().Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> UpdateAsync(T entity)
        {
            _db.Set<T>().Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _db.Set<T>().FindAsync(id) is not null;
    }
}
