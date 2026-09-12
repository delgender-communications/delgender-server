using Core.Enums;

namespace Core.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = null!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        public InvoiceStatus Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<InvoiceItem> Items { get; set; } = [];
    }
}
