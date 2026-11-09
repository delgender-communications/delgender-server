using Core.Enums;

namespace Core.DTOs.StaffRequests
{
    public class CreateStaffChangeRequestDto
    {
        public ChangeRequestType Type { get; set; }
        public string NewValue { get; set; } = string.Empty;
        public string? NewSurname { get; set; }
    }
}
