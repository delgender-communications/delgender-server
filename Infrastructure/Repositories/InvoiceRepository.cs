using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces.Repositories;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext db) : base(db) { }

        public async Task<Invoice?> GetByIdWithDetailsAsync(int id) =>
            await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.CreatedByStaff)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Invoice>> GetAllAsync(int page, int pageSize, InvoiceStatus? status, int? customerId)
        {
            var query = _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.CreatedByStaff)
                .Include(i => i.Items)
                .AsQueryable();

            if (status is not null)
            {
                query = query.Where(i => i.Status == status);
            }
            if (customerId is not null)
            {
                query = query.Where(i => i.CustomerId == customerId);
            }

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(InvoiceStatus? status, int? customerId)
        {
            var query = _db.Invoices.AsQueryable();
            if (status is not null)
            {
                query = query.Where(i => i.Status == status);
            }
            if (customerId is not null)
            {
                query = query.Where(i => i.CustomerId == customerId);
            }
            return await query.CountAsync();
        }

        public async Task<int> GetCountForYearAsync(int year) =>
            await _db.Invoices.CountAsync(i => i.IssueDate.Year == year);

        public async Task<List<Invoice>> GetOverdueCandidatesAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _db.Invoices
                .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate.Date < today)
                .ToListAsync();
        }
    }
}
