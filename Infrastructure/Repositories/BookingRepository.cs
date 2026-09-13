using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync(int page, int pageSize, BookingStatus? status)
        {
            var query = _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RespondedByStaff)
                .AsQueryable();

            if (status is not null) query = query.Where(b => b.Status == status);

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(BookingStatus? status)
        {
            var query = _db.Bookings.AsQueryable();
            if (status is not null) query = query.Where(b => b.Status == status);
            return await query.CountAsync();
        }

        public async Task<Booking?> GetByIdBookingAsync(int id) =>
            await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RespondedByStaff)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<string> GenerateNextReferenceAsync()
        {
            var count = await _db.Bookings.CountAsync();
            return $"#BK-{(count + 1):D4}";
        }
    }
}
