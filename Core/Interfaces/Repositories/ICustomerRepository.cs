using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByEmailAsync(string email);
        Task<List<Customer>> GetClientsAsync(int page, int pageSize, ClientStatus? status);
        Task<int> GetClientsCountAsync(ClientStatus? status);
    }
}
