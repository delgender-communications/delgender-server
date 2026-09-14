using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync(int page, int pageSize, BookingStatus? status);
        Task<int> GetTotalCountAsync(BookingStatus? status);
        Task<Booking?> GetByIdBookingAsync(int id);
        Task<int> GetNextBookingReferenceNumberAsync();
    }
}
