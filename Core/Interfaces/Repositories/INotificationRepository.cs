using Core.DTOs.Notifications;
using Core.Entities;

namespace Core.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        // False when a notification with the same dedupe key already exists.
        Task<bool> TryAddAsync(Notification notification);

        Task<(List<NotificationDto> Items, int Total)> GetPageAsync(
            NotificationAudience audience, int page, int pageSize, bool unreadOnly);

        Task<int> CountTotalAsync(NotificationAudience audience);
        Task<int> CountUnreadAsync(NotificationAudience audience);

        // Unread notifications created after the given moment.
        Task<int> CountUnreadSinceAsync(NotificationAudience audience, DateTime after);

        // False when the notification doesn't exist or this person can't see it.
        Task<bool> MarkReadAsync(NotificationAudience audience, int notificationId);
        Task<bool> MarkUnreadAsync(NotificationAudience audience, int notificationId);
        Task MarkAllReadAsync(NotificationAudience audience);
    }
}
