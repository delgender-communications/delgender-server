using Core.Enums;

namespace Core.Entities
{
    public class ResearchLead
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = null!;
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public ResearchLeadCase Case { get; set; } = ResearchLeadCase.Other;
        public int? Score { get; set; }
        public string? Summary { get; set; }

        // Used only to spot duplicates. WebsiteHost is "example.com" (no www), NameKey is
        // the company name lower-cased with punctuation and "Pty Ltd"-style endings removed.
        public string? WebsiteHost { get; set; }
        public string NameKey { get; set; } = null!;

        public ResearchLeadStatus Status { get; set; } = ResearchLeadStatus.New;
        public string? DismissReason { get; set; }

        public int? ReviewedByStaffId { get; set; }
        public Staff? ReviewedByStaff { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // Set when the lead was accepted into outreach.
        public int? OutreachCompanyId { get; set; }
        public OutreachCompany? OutreachCompany { get; set; }

        // Null means the research agent found it; otherwise the staff member who typed it in.
        public int? AddedByStaffId { get; set; }
        public Staff? AddedByStaff { get; set; }

        public string? RunId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ResearchLeadEvidence> Evidence { get; set; } = new List<ResearchLeadEvidence>();
    }
}
