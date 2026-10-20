namespace Core.DTOs.Outreach
{
    public class OutreachLogDto
    {
        public int Id { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
