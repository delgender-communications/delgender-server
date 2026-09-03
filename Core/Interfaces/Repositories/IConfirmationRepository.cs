using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface IConfirmationRepository : IRepository<Confirmation>
    {
        Task<IEnumerable<Confirmation>> GetAllForBookingAsync(int bookingId);
    }
}
