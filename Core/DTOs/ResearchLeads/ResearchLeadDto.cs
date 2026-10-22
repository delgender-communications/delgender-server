using Core.Enums;

namespace Core.DTOs.ResearchLeads
{
    public class ResearchLeadEvidenceDto
    {
        public string Finding { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }
    }

    public class ResearchLeadDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public ResearchLeadCase Case { get; set; }
        public int? Score { get; set; }
        public string? Summary { get; set; }

        public ResearchLeadStatus Status { get; set; }
        public string? DismissReason { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? OutreachCompanyId { get; set; }

        public string FoundBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<ResearchLeadEvidenceDto> Evidence { get; set; } = new();
    }
}
