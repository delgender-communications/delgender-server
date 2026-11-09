using Core.DTOs;
using Core.DTOs.Notifications;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private const int MaxPageSize = 50;

        private readonly INotificationRepository _notificationRepository;
        private readonly IStaffRepository _staffRepository;

        public NotificationService(INotificationRepository notificationRepository, IStaffRepository staffRepository)
        {
            _notificationRepository = notificationRepository;
            _staffRepository = staffRepository;
        }

        public Task NotifyEveryoneAsync(NotificationType type, string title, string? body, string? linkUrl, string? dedupeKey = null) =>
            CreateAsync(new Notification
            {
                Type = type,
                Title = title,
                Body = body,
                LinkUrl = linkUrl,
                DedupeKey = dedupeKey
            });

        public Task NotifyAdminsAsync(NotificationType type, string title, string? body, string? linkUrl) =>
            CreateAsync(new Notification
            {
                Type = type,
                Title = title,
                Body = body,
                LinkUrl = linkUrl,
                AdminOnly = true
            });

        public Task NotifyStaffAsync(int staffId, NotificationType type, string title, string? body, string? linkUrl) =>
            CreateAsync(new Notification
            {
                Type = type,
                Title = title,
                Body = body,
                LinkUrl = linkUrl,
                RecipientStaffId = staffId
            });

        private async Task CreateAsync(Notification notification)
        {
            try
            {
                await _notificationRepository.TryAddAsync(notification);
            }
            catch (Exception ex)
            {
                // The booking, invoice or lead that caused this has already been saved.
                // Losing the update is better than failing that.
                Console.WriteLine("Could not create notification: " + ex.Message);
            }
        }

        // ---- Reading ----

        private static NotificationAudience AudienceFor(Staff staff)
        {
            var types = new List<NotificationType>
            {
                NotificationType.ChangeRequest,
                NotificationType.ChangeRequestDecision
            };

            if (staff.NotifyNewLeads) types.Add(NotificationType.NewLead);
            if (staff.NotifyNewBookings) types.Add(NotificationType.NewBooking);
            if (staff.NotifyOverdueInvoices) types.Add(NotificationType.InvoiceOverdue);

            // Nothing from before they joined.
            return new NotificationAudience(staff.Id, staff.Role == StaffRole.Admin, staff.CreatedAt, types);
        }

        private async Task<Staff> RequireStaffAsync(int staffId) =>
            await _staffRepository.GetByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Staff member not found.");

        private async Task<NotificationSummaryDto> SummaryAsync(Staff staff)
        {
            var audience = AudienceFor(staff);
            var seenAt = staff.NotificationsSeenAt ?? staff.CreatedAt;

            return new NotificationSummaryDto
            {
                Badge = await _notificationRepository.CountUnreadSinceAsync(audience, seenAt),
                Unread = await _notificationRepository.CountUnreadAsync(audience),
                Total = await _notificationRepository.CountTotalAsync(audience)
            };
        }

        public async Task<NotificationSummaryDto> GetSummaryAsync(int staffId) =>
            await SummaryAsync(await RequireStaffAsync(staffId));

        public async Task<PagedResultDto<NotificationDto>> GetPageAsync(int staffId, int page, int pageSize, bool unreadOnly)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var staff = await RequireStaffAsync(staffId);
            var (items, total) = await _notificationRepository.GetPageAsync(AudienceFor(staff), page, pageSize, unreadOnly);

            return new PagedResultDto<NotificationDto>
            {
                Data = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ---- Opening Updates and reading notifications ----

        // The sidebar number and "unread" are two separate things. Opening Updates moves
        // this marker forward, so the number goes to zero, but nothing is marked as read.
        // The number only comes back for notifications that arrive after this moment.
        public async Task<NotificationSummaryDto> MarkSeenAsync(int staffId)
        {
            var staff = await RequireStaffAsync(staffId);

            staff.NotificationsSeenAt = DateTime.UtcNow;
            await _staffRepository.UpdateAsync(staff);

            return await SummaryAsync(staff);
        }

        public async Task<bool> MarkReadAsync(int staffId, int notificationId) =>
            await _notificationRepository.MarkReadAsync(AudienceFor(await RequireStaffAsync(staffId)), notificationId);

        public async Task<bool> MarkUnreadAsync(int staffId, int notificationId) =>
            await _notificationRepository.MarkUnreadAsync(AudienceFor(await RequireStaffAsync(staffId)), notificationId);

        public async Task MarkAllReadAsync(int staffId) =>
            await _notificationRepository.MarkAllReadAsync(AudienceFor(await RequireStaffAsync(staffId)));

        // ---- Preferences ----

        public async Task<NotificationPreferencesDto> GetPreferencesAsync(int staffId) =>
            ToPreferences(await RequireStaffAsync(staffId));

        public async Task<NotificationPreferencesDto> UpdatePreferencesAsync(int staffId, NotificationPreferencesDto dto)
        {
            var staff = await RequireStaffAsync(staffId);

            staff.NotifyNewLeads = dto.NotifyNewLeads;
            staff.NotifyNewBookings = dto.NotifyNewBookings;
            staff.NotifyOverdueInvoices = dto.NotifyOverdueInvoices;

            await _staffRepository.UpdateAsync(staff);
            return ToPreferences(staff);
        }

        private static NotificationPreferencesDto ToPreferences(Staff staff) => new()
        {
            NotifyNewLeads = staff.NotifyNewLeads,
            NotifyNewBookings = staff.NotifyNewBookings,
            NotifyOverdueInvoices = staff.NotifyOverdueInvoices
        };
    }
}
