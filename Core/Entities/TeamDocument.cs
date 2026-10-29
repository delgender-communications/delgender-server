namespace Core.Entities
{
    public class TeamDocument
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public string FileUrl { get; set; } = null!;
        public string FilePublicId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public long SizeBytes { get; set; }

        public int UploadedByStaffId { get; set; }
        public Staff UploadedByStaff { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
