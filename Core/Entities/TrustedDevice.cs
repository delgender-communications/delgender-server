namespace Core.Entities
{
    public class TrustedDevice
    {
        public int Id { get; set; }

        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        public string TokenHash { get; set; } = null!;
        public string? Label { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
