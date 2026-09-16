using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext db) : base(db) { }

        public async Task<Customer?> GetByEmailAsync(string email) =>
            await _db.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

        public async Task<List<Customer>> GetClientsAsync(int page, int pageSize, ClientStatus? status)
        {
            var query = _db.Customers.AsQueryable();

            if (status is not null)
            {
                query = query.Where(c => c.Status == status);
            }

            query = query
                .OrderBy(c => c.Status == ClientStatus.Working ? 0 : c.Status == ClientStatus.Pending ? 1 : 2)
                .ThenByDescending(c => c.UpdatedAt);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetClientsCountAsync(ClientStatus? status)
        {
            var query = _db.Customers.AsQueryable();
            if (status is not null)
            {
                query = query.Where(c => c.Status == status);
            }
            return await query.CountAsync();
        }
    }
}
