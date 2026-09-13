using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Invoice>> GetAllAsync(int page, int pageSize, InvoiceStatus? status, int? customerId);
        Task<int> GetTotalCountAsync(InvoiceStatus? status, int? customerId);
        Task<int> GetCountForYearAsync(int year);
    }
}
