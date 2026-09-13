using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Invoice
{
    public class UpdateInvoiceStatusDto
    {
        [Required]
        public InvoiceStatus Status { get; set; }

        public string? PaymentReference { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
