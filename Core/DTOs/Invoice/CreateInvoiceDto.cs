using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Invoice
{
    public class CreateInvoiceDto
    {
        [Required]
        public int CustomerId { get; set; }

        public int? BookingId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<InvoiceItemInputDto> Items { get; set; } = new();
    }
}
