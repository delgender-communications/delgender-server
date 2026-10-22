using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.ResearchLeads
{
    public class DismissResearchLeadDto
    {
        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
