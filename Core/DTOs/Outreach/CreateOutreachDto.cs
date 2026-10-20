using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Outreach
{
    public class CreateOutreachDto
    {
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(120)]
        public string? ContactName { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [StringLength(4000)]
        public string? Notes { get; set; }

        [Required]
        public OutreachStatus Status { get; set; } = OutreachStatus.Researching;
    }
}
