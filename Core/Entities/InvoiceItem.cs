namespace Core.Entities
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
