using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.ResearchLeads
{
    public class ResearchLeadEvidenceInputDto
    {
        [Required, StringLength(500)]
        public string Finding { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SourceUrl { get; set; }
    }

    public class ResearchLeadInputDto
    {
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Website { get; set; }

        [StringLength(120)]
        public string? Industry { get; set; }

        [StringLength(120)]
        public string? Location { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        public ResearchLeadCase Case { get; set; } = ResearchLeadCase.Other;

        // 0 to 100. Optional, because a lead typed in by hand has no score.
        public int? Score { get; set; }

        [StringLength(1000)]
        public string? Summary { get; set; }

        public List<ResearchLeadEvidenceInputDto> Evidence { get; set; } = new();
    }
}
