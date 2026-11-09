using Core.Enums;

namespace Core.Entities
{
    public class StaffChangeRequest
    {
        public int Id { get; set; }

        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;

        public ChangeRequestType Type { get; set; }

        public string OldValue { get; set; } = null!;

        public string NewValue { get; set; } = null!;
        public string? NewSurname { get; set; }

        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByStaffId { get; set; }
        public Staff? ReviewedByStaff { get; set; }
        public string? ReviewNote { get; set; }
    }
}
