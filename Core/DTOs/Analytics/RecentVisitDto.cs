namespace Core.DTOs.Analytics
{
    public class RecentVisitDto
    {
        public DateTime VisitedAt { get; set; }
        public string VisitorId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Referrer { get; set; } = "Direct";
    }
}
