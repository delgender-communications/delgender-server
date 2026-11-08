using Core.Enums;

namespace Core.DTOs.StaffRequests
{
    public class StaffChangeRequestDto
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffCode { get; set; } = string.Empty;

        public ChangeRequestType Type { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;

        public ChangeRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByName { get; set; }
        public string? ReviewNote { get; set; }
    }
}
