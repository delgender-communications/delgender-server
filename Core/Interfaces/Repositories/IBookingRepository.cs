using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<Booking?> GetByIdBookingAsync(int id);
    }
}
