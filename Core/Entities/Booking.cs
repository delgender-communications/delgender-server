using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [Required, StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Industry { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string HelpWith { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string ProblemDescription { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string SessionGoal { get; set; } = string.Empty;

        [Required]
        public MeetingType Meeting { get; set; } = MeetingType.InPerson;

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }
        public bool ContactPermission { get; set; } = false;

        public Confirmation Confirmation { get; set; } = null!;
    }
}
