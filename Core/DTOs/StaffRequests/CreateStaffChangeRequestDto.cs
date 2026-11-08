using Core.Enums;

namespace Core.DTOs.StaffRequests
{
    public class CreateStaffChangeRequestDto
    {
        public ChangeRequestType Type { get; set; }

        // The new email or phone number, or the new first name for a name change.
        public string NewValue { get; set; } = string.Empty;

        // Only for a name change.
        public string? NewSurname { get; set; }
    }
}
