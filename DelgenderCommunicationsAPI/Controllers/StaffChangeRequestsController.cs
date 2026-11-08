using Core.DTOs.StaffRequests;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DelgenderCommunicationsAPI.Controllers
{
    /// <summary>
    /// Staff can't edit their own name, email or phone number. They ask here, and an
    /// admin approves or declines. Nothing changes until it's approved.
    /// </summary>
    [ApiController]
    [Route("api/v1/staff")]
    [Authorize]
    public class StaffChangeRequestsController : Controller
    {
        private readonly IStaffChangeService _changeService;

        public StaffChangeRequestsController(IStaffChangeService changeService)
        {
            _changeService = changeService;
        }

        private int StaffId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me/change-requests")]
        public async Task<ActionResult<List<StaffChangeRequestDto>>> GetMine()
        {
            return Ok(await _changeService.GetMineAsync(StaffId));
        }

        [HttpPost("me/change-requests")]
        public async Task<ActionResult<StaffChangeRequestDto>> Create([FromBody] CreateStaffChangeRequestDto dto)
        {
            return Ok(await _changeService.CreateAsync(StaffId, dto));
        }

        [HttpDelete("me/change-requests/{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            return await _changeService.CancelAsync(StaffId, id) ? NoContent() : NotFound();
        }

        // Admin: what needs a decision. Pass ?status=Approved (or Rejected, Cancelled) for history.
        [HttpGet("change-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<StaffChangeRequestDto>>> GetByStatus(
            [FromQuery] ChangeRequestStatus? status = ChangeRequestStatus.Pending)
        {
            return Ok(await _changeService.GetByStatusAsync(status));
        }

        [HttpPost("change-requests/{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StaffChangeRequestDto>> Approve(int id, [FromBody] ReviewStaffChangeRequestDto dto)
        {
            return Ok(await _changeService.ApproveAsync(StaffId, id, dto.Note));
        }

        [HttpPost("change-requests/{id:int}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StaffChangeRequestDto>> Reject(int id, [FromBody] ReviewStaffChangeRequestDto dto)
        {
            return Ok(await _changeService.RejectAsync(StaffId, id, dto.Note));
        }
    }
}
