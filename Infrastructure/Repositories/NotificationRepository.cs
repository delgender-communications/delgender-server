using Core.DTOs.Notifications;
using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db) => _db = db;

        public async Task<bool> TryAddAsync(Notification notification)
        {
            if (notification.DedupeKey is not null &&
                await _db.Notifications.AnyAsync(n => n.DedupeKey == notification.DedupeKey))
            {
                return false;
            }

            _db.Notifications.Add(notification);

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException) when (notification.DedupeKey is not null)
            {
                // Someone else created the same event a moment ago.
                _db.Entry(notification).State = EntityState.Detached;
                return false;
            }
        }

        // Everything this person is allowed to see.
        private IQueryable<Notification> Visible(NotificationAudience audience)
        {
            var types = audience.Types.ToList();
            var staffId = audience.StaffId;
            var isAdmin = audience.IsAdmin;
            var since = audience.Since;

            return _db.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.CreatedAt >= since &&
                    types.Contains(n.Type) &&
                    (n.RecipientStaffId == staffId ||
                     (n.RecipientStaffId == null && (!n.AdminOnly || isAdmin))));
        }

        private IQueryable<Notification> Unread(NotificationAudience audience)
        {
            var staffId = audience.StaffId;

            return Visible(audience).Where(n =>
                !_db.NotificationReads.Any(r => r.NotificationId == n.Id && r.StaffId == staffId));
        }

        public async Task<(List<NotificationDto> Items, int Total)> GetPageAsync(
            NotificationAudience audience, int page, int pageSize, bool unreadOnly)
        {
            var staffId = audience.StaffId;
            var query = unreadOnly ? Unread(audience) : Visible(audience);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Body = n.Body,
                    LinkUrl = n.LinkUrl,
                    CreatedAt = n.CreatedAt,
                    IsRead = _db.NotificationReads.Any(r => r.NotificationId == n.Id && r.StaffId == staffId)
                })
                .ToListAsync();

            return (items, total);
        }

        public Task<int> CountTotalAsync(NotificationAudience audience) =>
            Visible(audience).CountAsync();

        public Task<int> CountUnreadAsync(NotificationAudience audience) =>
            Unread(audience).CountAsync();

        public Task<int> CountUnreadSinceAsync(NotificationAudience audience, DateTime after) =>
            Unread(audience).Where(n => n.CreatedAt > after).CountAsync();

        public async Task<bool> MarkReadAsync(NotificationAudience audience, int notificationId)
        {
            if (!await Visible(audience).AnyAsync(n => n.Id == notificationId)) return false;

            var staffId = audience.StaffId;
            if (await _db.NotificationReads.AnyAsync(r => r.NotificationId == notificationId && r.StaffId == staffId))
            {
                return true;
            }

            var read = new NotificationRead { NotificationId = notificationId, StaffId = staffId };
            _db.NotificationReads.Add(read);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _db.Entry(read).State = EntityState.Detached;
            }

            return true;
        }

        public async Task<bool> MarkUnreadAsync(NotificationAudience audience, int notificationId)
        {
            if (!await Visible(audience).AnyAsync(n => n.Id == notificationId)) return false;

            var staffId = audience.StaffId;
            await _db.NotificationReads
                .Where(r => r.NotificationId == notificationId && r.StaffId == staffId)
                .ExecuteDeleteAsync();

            return true;
        }

        public async Task MarkAllReadAsync(NotificationAudience audience)
        {
            var ids = await Unread(audience).Select(n => n.Id).ToListAsync();
            if (ids.Count == 0) return;

            _db.NotificationReads.AddRange(ids.Select(id => new NotificationRead
            {
                NotificationId = id,
                StaffId = audience.StaffId
            }));

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                foreach (var id in ids) await MarkReadAsync(audience, id);
            }
        }
    }
}
