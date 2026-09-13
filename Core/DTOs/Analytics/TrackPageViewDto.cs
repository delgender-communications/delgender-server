using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Analytics
{
    public class TrackPageViewDto
    {
        [Required, StringLength(100)]
        public string VisitorId { get; set; } = string.Empty;

        [Required, StringLength(300)]
        public string Path { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Referrer { get; set; }
    }
}
