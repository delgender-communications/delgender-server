namespace Core.DTOs.Auth
{
    public class TrustedDeviceDto
    {
        public int Id { get; set; }
        public string? Label { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
    }
}
