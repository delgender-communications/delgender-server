using Core.Enums;

namespace Core.DTOs.Outreach
{
    public class OutreachDetailDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public OutreachStatus Status { get; set; }

        public string? PitchDeckFileName { get; set; }
        public string? PitchDeckUrl { get; set; }
        public DateTime? PitchDeckUploadedAt { get; set; }

        public string? ReportFileName { get; set; }
        public string? ReportUrl { get; set; }
        public DateTime? ReportUploadedAt { get; set; }

        public int AddedByStaffId { get; set; }
        public string AddedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<OutreachLogDto> Logs { get; set; } = new();
    }
}
