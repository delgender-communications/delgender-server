using Core.Enums;

namespace Core.DTOs.Notifications
{
    public record NotificationAudience(
        int StaffId,
        bool IsAdmin,
        DateTime Since,
        IReadOnlyCollection<NotificationType> Types);
}
