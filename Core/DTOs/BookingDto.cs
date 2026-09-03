using Core.Enums;

namespace Core.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string HelpWith { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string SessionGoal { get; set; } = string.Empty;
        public MeetingType Meeting { get; set; } = MeetingType.InPerson;
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public bool ContactPermission { get; set; } = false;
    }
}
