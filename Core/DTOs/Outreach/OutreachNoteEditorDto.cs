namespace Core.DTOs.Outreach
{
    public class OutreachNoteEditorDto
    {
        public int StaffId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int EditCount { get; set; }
        public DateTime LastEditedAt { get; set; }
    }
}
