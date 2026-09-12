namespace Core.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;
        public string? JobTitle { get; set; }

        public string CompanyName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public string Industry { get; set; } = null!;

        public bool ContactPermission { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
