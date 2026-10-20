using Core.Enums;

namespace Core.DTOs.Outreach
{
    public class OutreachListItemDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public OutreachStatus Status { get; set; }
        public string? PitchDeckFileName { get; set; }
        public string? PitchDeckUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
