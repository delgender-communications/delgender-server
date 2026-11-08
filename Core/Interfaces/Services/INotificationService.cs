using Core.DTOs;
using Core.DTOs.Notifications;
using Core.Enums;

namespace Core.Interfaces.Services
{
    public interface INotificationService
    {
        // Creating a notification never throws. A failure here must not break the
        // booking, invoice or lead that caused it.
        Task NotifyEveryoneAsync(NotificationType type, string title, string? body, string? linkUrl, string? dedupeKey = null);
        Task NotifyAdminsAsync(NotificationType type, string title, string? body, string? linkUrl);
        Task NotifyStaffAsync(int staffId, NotificationType type, string title, string? body, string? linkUrl);

        Task<NotificationSummaryDto> GetSummaryAsync(int staffId);
        Task<PagedResultDto<NotificationDto>> GetPageAsync(int staffId, int page, int pageSize, bool unreadOnly);

        // Opening Updates. Clears the sidebar number and nothing else.
        Task<NotificationSummaryDto> MarkSeenAsync(int staffId);

        Task<bool> MarkReadAsync(int staffId, int notificationId);
        Task<bool> MarkUnreadAsync(int staffId, int notificationId);
        Task MarkAllReadAsync(int staffId);

        Task<NotificationPreferencesDto> GetPreferencesAsync(int staffId);
        Task<NotificationPreferencesDto> UpdatePreferencesAsync(int staffId, NotificationPreferencesDto dto);
    }
}
