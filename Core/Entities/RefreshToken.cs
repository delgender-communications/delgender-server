namespace Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        public string TokenHash { get; set; } = null!;
        public bool RememberMe { get; set; }

        public string? CreatedByIp { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    }
}
