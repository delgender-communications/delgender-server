using Core.Enums;

namespace Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }

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
    }
}
