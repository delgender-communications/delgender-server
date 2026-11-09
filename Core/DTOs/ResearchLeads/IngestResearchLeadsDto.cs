using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.ResearchLeads
{
    public class IngestResearchLeadsDto
    {
        [StringLength(100)]
        public string? RunId { get; set; }

        public List<ResearchLeadInputDto> Leads { get; set; } = new();
    }
}
