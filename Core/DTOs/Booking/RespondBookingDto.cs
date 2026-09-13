using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Booking
{
    public class RespondBookingDto
    {
        [Required]
        public BookingStatus Status { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DeclineReason { get; set; }
    }
}
