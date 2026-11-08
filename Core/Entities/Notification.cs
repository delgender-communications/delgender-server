using Core.Enums;

namespace Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }

        public string? LinkUrl { get; set; }

        public int? RecipientStaffId { get; set; }
        public bool AdminOnly { get; set; }

        public string? DedupeKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
