using Core.Entities;

namespace Core.Interfaces.Services
{
    public interface IInvoicePdfService
    {
        byte[] Generate(Invoice invoice);
    }
}
