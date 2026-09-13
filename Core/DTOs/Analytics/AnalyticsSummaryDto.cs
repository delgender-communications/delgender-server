namespace Core.DTOs.Analytics
{
    public class AnalyticsSummaryDto
    {
        public DateOnly Date { get; set; }

        public int Visitors { get; set; }
        public int PageViews { get; set; }

        public double VisitorsChangePct { get; set; }
        public double PageViewsChangePct { get; set; }

        public List<HourlyPointDto> Series { get; set; } = new();
        public List<TopPageDto> TopPages { get; set; } = new();
        public List<RecentVisitDto> RecentVisits { get; set; } = new();
        public int TotalVisitsForDay { get; set; }
    }
}
