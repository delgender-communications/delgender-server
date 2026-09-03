using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext db) : base(db) { }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync(int page, int pageSize) =>
            await _db.Bookings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync() =>
            await _db.Bookings.CountAsync();

        public async Task<Booking?> GetByIdBookingAsync(int id) =>
            await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);
    }
}
