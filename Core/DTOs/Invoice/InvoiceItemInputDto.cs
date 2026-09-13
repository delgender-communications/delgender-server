using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Invoice
{
    public class InvoiceItemInputDto
    {
        [Required, StringLength(300)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
    }
}
