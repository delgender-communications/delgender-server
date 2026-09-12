namespace Core.Entities
{
    public class PageView
    {
        public int Id { get; set; }
        public string VisitorId { get; set; } = null!;
        public string Path { get; set; } = null!;
        public string? Referrer { get; set; }
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
