namespace Core.DTOs.Documents
{
    public class DocumentsSentDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
    }
}
