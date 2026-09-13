using Core.DTOs;
using Core.DTOs.Analytics;

namespace Core.Interfaces.Services
{
    public interface IAnalyticsService
    {
        Task TrackAsync(TrackPageViewDto dto);
        Task<AnalyticsSummaryDto> GetSummaryAsync(DateOnly date);
        Task<PagedResultDto<RecentVisitDto>> GetRecentVisitsAsync(int page, int pageSize);
    }
}
