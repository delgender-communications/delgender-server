using Core.Enums;

namespace Core.Entities
{
    public class Confirmation
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public ConfirmationStatus Status { get; set; } = ConfirmationStatus.Pending;
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
