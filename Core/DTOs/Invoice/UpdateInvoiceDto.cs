using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Invoice
{
    public class UpdateInvoiceDto
    {
        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<InvoiceItemInputDto> Items { get; set; } = new();
    }
}
