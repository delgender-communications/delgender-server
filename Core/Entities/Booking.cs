using Core.Enums;

namespace Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        public int BookingNumber { get; set; }
        public string BookingReference { get; set; } = null!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public string HelpWith { get; set; } = null!;
        public string ProblemDescription { get; set; } = null!;
        public string SessionGoal { get; set; } = null!;

        public MeetingType Meeting { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Confirmation Confirmation { get; set; } = null!;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public int? RespondedByStaffId { get; set; }
        public Staff? RespondedByStaff { get; set; }

        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public string? DeclineReason { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
