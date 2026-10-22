using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.ResearchLeads
{
    public class IngestResearchLeadsDto
    {
        // Free-form label for one run of the agent, so leads can be traced back to it.
        [StringLength(100)]
        public string? RunId { get; set; }

        public List<ResearchLeadInputDto> Leads { get; set; } = new();
    }
}
