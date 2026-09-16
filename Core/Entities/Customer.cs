using Core.Enums;

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
        public string? IdNumber { get; set; }

        public bool ContactPermission { get; set; } = false;

        public ClientStatus Status { get; set; } = ClientStatus.Pending;
        public DateTime? WorkingSince { get; set; }
        public DateTime? WorkingUntil { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Feedback> FeedbackRequests { get; set; } = new List<Feedback>();
    }
}
