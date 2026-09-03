using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Confirmation
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public ConfirmationStatus Status { get; set; } = ConfirmationStatus.Pending;
        public DateTime? SentAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
