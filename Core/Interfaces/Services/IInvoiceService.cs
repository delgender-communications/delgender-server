using Core.DTOs;
using Core.DTOs.Invoice;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, int staffId);
        Task<InvoiceDto?> UpdateAsync(int id, UpdateInvoiceDto dto);
        Task<InvoiceDto?> GetByIdAsync(int id);
        Task<PagedResultDto<InvoiceDto>> GetAllAsync(int page, int pageSize, InvoiceStatus? status, int? customerId);
        Task<InvoiceDto?> UpdateStatusAsync(int id, UpdateInvoiceStatusDto dto);
        Task<InvoiceDto?> SendAsync(int id, SendInvoiceDto dto);
        Task<int> RefreshOverdueInvoicesAsync();
        Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int id);
    }
}
