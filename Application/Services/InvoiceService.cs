using Core.DTOs;
using Core.DTOs.Invoice;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;
        private readonly IInvoicePdfService _pdfService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            IEmailService emailService,
            IInvoicePdfService pdfService)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, int staffId)
        {
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var invoice = new Invoice
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                CustomerId = customer.Id,
                BookingId = dto.BookingId,
                CreatedByStaffId = staffId,
                IssueDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Status = InvoiceStatus.Draft,
                Notes = dto.Notes,
                Items = BuildItems(dto.Items)
            };

            ApplyTotals(invoice);

            invoice = await _invoiceRepository.CreateAsync(invoice);
            var created = await _invoiceRepository.GetByIdWithDetailsAsync(invoice.Id);

            return ToDto(created!);
        }

        public async Task<InvoiceDto?> UpdateAsync(int id, UpdateInvoiceDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice is null)
            {
                return null;
            }

            if (invoice.Status != InvoiceStatus.Draft)
            {
                throw new InvalidOperationException("Only draft invoices can be edited. Once an invoice is sent, it's locked.");
            }

            invoice.DueDate = dto.DueDate;
            invoice.Notes = dto.Notes;
            invoice.Items = BuildItems(dto.Items);
            invoice.UpdatedAt = DateTime.UtcNow;

            ApplyTotals(invoice);

            await _invoiceRepository.UpdateAsync(invoice);
            return ToDto(invoice);
        }

        public async Task<InvoiceDto?> GetByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            return invoice is null ? null : ToDto(invoice);
        }

        public async Task<PagedResultDto<InvoiceDto>> GetAllAsync(int page, int pageSize, InvoiceStatus? status, int? customerId)
        {
            var invoices = await _invoiceRepository.GetAllAsync(page, pageSize, status, customerId);
            var total = await _invoiceRepository.GetTotalCountAsync(status, customerId);

            return new PagedResultDto<InvoiceDto>
            {
                Data = invoices.Select(ToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<InvoiceDto?> UpdateStatusAsync(int id, UpdateInvoiceStatusDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice is null)
            {
                return null;
            }

            invoice.Status = dto.Status;
            invoice.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == InvoiceStatus.Paid)
            {
                invoice.PaidAt = dto.PaidAt ?? DateTime.UtcNow;
                invoice.PaymentReference = dto.PaymentReference;
            }
            else
            {
                invoice.PaidAt = null;
            }

            await _invoiceRepository.UpdateAsync(invoice);
            return ToDto(invoice);
        }

        public async Task<InvoiceDto?> SendAsync(int id, SendInvoiceDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice is null)
            {
                return null;
            }

            var pdfBytes = _pdfService.Generate(invoice);
            await _emailService.SendInvoiceAsync(invoice, dto.Message, pdfBytes);

            if (invoice.Status == InvoiceStatus.Draft)
            {
                invoice.Status = InvoiceStatus.Sent;
                invoice.UpdatedAt = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            return ToDto(invoice);
        }

        public async Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int id)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
            if (invoice is null) return null;

            var bytes = _pdfService.Generate(invoice);
            return (bytes, $"{invoice.InvoiceNumber}.pdf");
        }

        public async Task<int> RefreshOverdueInvoicesAsync()
        {
            var candidates = await _invoiceRepository.GetOverdueCandidatesAsync();

            foreach (var invoice in candidates)
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);
            }

            return candidates.Count;
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _invoiceRepository.GetCountForYearAsync(year);
            return $"INV-{year}-{(count + 1):D4}";
        }

        private static List<InvoiceItem> BuildItems(List<InvoiceItemInputDto> items) =>
            items.Select(i =>
            {
                var lineSubtotal = i.Quantity * i.UnitPrice;
                var taxable = lineSubtotal - i.DiscountAmount;
                var tax = taxable * (i.TaxRate / 100m);

                return new InvoiceItem
                {
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxRate = i.TaxRate,
                    DiscountAmount = i.DiscountAmount,
                    TotalAmount = taxable + tax
                };
            }).ToList();

        private static void ApplyTotals(Invoice invoice)
        {
            invoice.Subtotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.DiscountAmount = invoice.Items.Sum(i => i.DiscountAmount);
            invoice.TaxAmount = invoice.Items.Sum(i => (i.Quantity * i.UnitPrice - i.DiscountAmount) * (i.TaxRate / 100m));
            invoice.TotalAmount = invoice.Items.Sum(i => i.TotalAmount);
        }

        private static InvoiceDto ToDto(Invoice invoice)
        {
            var displayStatus = invoice.Status == InvoiceStatus.Sent && invoice.DueDate.Date < DateTime.UtcNow.Date
                ? InvoiceStatus.Overdue
                : invoice.Status;

            return new()
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.FullName ?? string.Empty,
                CustomerEmail = invoice.Customer?.Email ?? string.Empty,
                CustomerCompany = invoice.Customer?.CompanyName ?? string.Empty,
                BookingId = invoice.BookingId,
                CreatedByStaffName = invoice.CreatedByStaff is null ? null : $"{invoice.CreatedByStaff.Name} {invoice.CreatedByStaff.Surname}",
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Status = displayStatus,
                Subtotal = invoice.Subtotal,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,
                PaidAt = invoice.PaidAt,
                PaymentReference = invoice.PaymentReference,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                Items = invoice.Items.Select(i => new InvoiceItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TaxRate = i.TaxRate,
                    DiscountAmount = i.DiscountAmount,
                    TotalAmount = i.TotalAmount
                }).ToList()
            };
        }
    }
}
