using Core.Enums;

namespace Core.Entities
{
    public class OutreachCompany
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = null!;
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }

        public OutreachStatus Status { get; set; } = OutreachStatus.Researching;

        public string? PitchDeckUrl { get; set; }
        public string? PitchDeckPublicId { get; set; }
        public string? PitchDeckFileName { get; set; }
        public DateTime? PitchDeckUploadedAt { get; set; }

        public string? ReportUrl { get; set; }
        public string? ReportPublicId { get; set; }
        public string? ReportFileName { get; set; }
        public DateTime? ReportUploadedAt { get; set; }

        public int AddedByStaffId { get; set; }
        public Staff AddedByStaff { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OutreachLog> Logs { get; set; } = new List<OutreachLog>();

    }
}
