using Core.DTOs;
using Core.DTOs.Notifications;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int StaffId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<NotificationDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool unreadOnly = false)
        {
            return Ok(await _notificationService.GetPageAsync(StaffId, page, pageSize, unreadOnly));
        }

        // The sidebar polls this. It's cheap: three counts.
        [HttpGet("summary")]
        public async Task<ActionResult<NotificationSummaryDto>> GetSummary()
        {
            return Ok(await _notificationService.GetSummaryAsync(StaffId));
        }

        // Called when Updates is opened. Clears the sidebar number only. Nothing is
        // marked as read.
        [HttpPost("seen")]
        public async Task<ActionResult<NotificationSummaryDto>> MarkSeen()
        {
            return Ok(await _notificationService.MarkSeenAsync(StaffId));
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            return await _notificationService.MarkReadAsync(StaffId, id) ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/unread")]
        public async Task<IActionResult> MarkUnread(int id)
        {
            return await _notificationService.MarkUnreadAsync(StaffId, id) ? NoContent() : NotFound();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(StaffId);
            return NoContent();
        }

        [HttpGet("preferences")]
        public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
        {
            return Ok(await _notificationService.GetPreferencesAsync(StaffId));
        }

        [HttpPut("preferences")]
        public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences([FromBody] NotificationPreferencesDto dto)
        {
            return Ok(await _notificationService.UpdatePreferencesAsync(StaffId, dto));
        }
    }
}
