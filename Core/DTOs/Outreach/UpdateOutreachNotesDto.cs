using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Outreach
{
    public class UpdateOutreachNotesDto
    {
        [StringLength(4000)]
        public string? Notes { get; set; }
    }
}
