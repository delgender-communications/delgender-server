namespace Core.DTOs.Documents
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public int UploadedByStaffId { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
