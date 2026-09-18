using Core.Enums;

namespace Core.DTOs.Booking
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string BookingReference { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = "";
        public string Industry { get; set; } = string.Empty;
        public string HelpWith { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string SessionGoal { get; set; } = string.Empty;
        public MeetingType Meeting { get; set; } = MeetingType.InPerson;
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public bool ContactPermission { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? RespondedByStaffName { get; set; }
        public string? RespondedByStaffProfilePictureUrl { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public string? DeclineReason { get; set; }
    }
}
