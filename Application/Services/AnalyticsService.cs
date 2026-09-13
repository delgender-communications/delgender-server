using Core.DTOs;
using Core.DTOs.Analytics;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IPageViewRepository _pageViewRepository;

        public AnalyticsService(IPageViewRepository pageViewRepository)
        {
            _pageViewRepository = pageViewRepository;
        }

        public async Task TrackAsync(TrackPageViewDto dto)
        {
            await _pageViewRepository.CreateAsync(new PageView
            {
                VisitorId = dto.VisitorId,
                Path = dto.Path,
                Referrer = string.IsNullOrWhiteSpace(dto.Referrer) ? null : dto.Referrer
            });
        }

        public async Task<AnalyticsSummaryDto> GetSummaryAsync(DateOnly date)
        {
            var todayViews = await _pageViewRepository.GetForDateAsync(date);
            var yesterdayViews = await _pageViewRepository.GetForDateAsync(date.AddDays(-1));

            var visitors = todayViews.Select(v => v.VisitorId).Distinct().Count();
            var pageViews = todayViews.Count;

            var prevVisitors = yesterdayViews.Select(v => v.VisitorId).Distinct().Count();
            var prevPageViews = yesterdayViews.Count;

            var series = Enumerable.Range(0, 24).Select(hour =>
            {
                var bucket = todayViews.Where(v => v.VisitedAt.Hour == hour).ToList();
                return new HourlyPointDto
                {
                    Hour = hour,
                    Visitors = bucket.Select(v => v.VisitorId).Distinct().Count(),
                    PageViews = bucket.Count
                };
            }).ToList();

            var topPages = todayViews
                .GroupBy(v => v.Path)
                .Select(g => new TopPageDto { Path = g.Key, PageViews = g.Count() })
                .OrderByDescending(p => p.PageViews)
                .Take(6)
                .ToList();

            var recentVisits = todayViews
                .OrderByDescending(v => v.VisitedAt)
                .Take(5)
                .Select(v => new RecentVisitDto
                {
                    VisitedAt = v.VisitedAt,
                    VisitorId = v.VisitorId,
                    Path = v.Path,
                    Referrer = string.IsNullOrWhiteSpace(v.Referrer) ? "Direct" : v.Referrer
                })
                .ToList();

            return new AnalyticsSummaryDto
            {
                Date = date,
                Visitors = visitors,
                PageViews = pageViews,
                VisitorsChangePct = PercentChange(prevVisitors, visitors),
                PageViewsChangePct = PercentChange(prevPageViews, pageViews),
                Series = series,
                TopPages = topPages,
                RecentVisits = recentVisits,
                TotalVisitsForDay = todayViews.Count
            };
        }

        public async Task<PagedResultDto<RecentVisitDto>> GetRecentVisitsAsync(int page, int pageSize)
        {
            var views = await _pageViewRepository.GetPagedAsync(page, pageSize);
            var total = await _pageViewRepository.GetTotalCountAsync();

            return new PagedResultDto<RecentVisitDto>
            {
                Data = views.Select(v => new RecentVisitDto
                {
                    VisitedAt = v.VisitedAt,
                    VisitorId = v.VisitorId,
                    Path = v.Path,
                    Referrer = string.IsNullOrWhiteSpace(v.Referrer) ? "Direct" : v.Referrer
                }),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static double PercentChange(int previous, int current)
        {
            if (previous == 0) return current == 0 ? 0 : 100;
            return Math.Round((current - previous) / (double)previous * 100, 1);
        }
    }
}
