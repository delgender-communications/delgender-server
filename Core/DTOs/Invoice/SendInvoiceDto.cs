using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Invoice
{
    public class SendInvoiceDto
    {
        [StringLength(1000)]
        public string? Message { get; set; }
    }
}
