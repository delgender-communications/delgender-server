using Core.DTOs.ResearchLeads;
using Core.Enums;

namespace Core.DTOs.Outreach
{
    public class OutreachResearchDto
    {
        public int LeadId { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }

        public ResearchLeadCase Case { get; set; }
        public int? Score { get; set; }
        public string? Summary { get; set; }

        public string FoundBy { get; set; } = string.Empty;
        public DateTime FoundAt { get; set; }

        public List<ResearchLeadEvidenceDto> Evidence { get; set; } = new();
    }
}
