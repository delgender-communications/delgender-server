using Core.DTOs;
using Core.DTOs.Analytics;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DelgenderCommunicationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpPost("track")]
        [AllowAnonymous]
        [EnableRateLimiting("booking")]
        public async Task<IActionResult> Track([FromBody] TrackPageViewDto dto)
        {
            await _analyticsService.TrackAsync(dto);
            return NoContent();
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary([FromQuery] DateOnly? date = null)
        {
            var result = await _analyticsService.GetSummaryAsync(date ?? DateOnly.FromDateTime(DateTime.UtcNow));
            return Ok(result);
        }

        [HttpGet("recent-visits")]
        [Authorize]
        public async Task<ActionResult<PagedResultDto<RecentVisitDto>>> GetRecentVisits(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _analyticsService.GetRecentVisitsAsync(page, pageSize);
            return Ok(result);
        }
    }
}
