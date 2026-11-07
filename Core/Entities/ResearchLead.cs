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

        public bool NeedsResearch { get; set; }
        public string? ResearchReason { get; set; }

        public string? SiteStack { get; set; }

        public string? WebsiteHost { get; set; }
        public string NameKey { get; set; } = null!;

        public ResearchLeadStatus Status { get; set; } = ResearchLeadStatus.New;
        public string? DismissReason { get; set; }

        public int? ReviewedByStaffId { get; set; }
        public Staff? ReviewedByStaff { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public int? OutreachCompanyId { get; set; }
        public OutreachCompany? OutreachCompany { get; set; }

        public int? AddedByStaffId { get; set; }
        public Staff? AddedByStaff { get; set; }

        public string? RunId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ResearchLeadEvidence> Evidence { get; set; } = new List<ResearchLeadEvidence>();
    }
}
