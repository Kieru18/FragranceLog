using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class CommunityMoodInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public CommunityMoodInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Global;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddHours(-24);

            var stats = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewDate >= since)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Avg = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .SingleOrDefaultAsync(ct);

            if (stats == null || stats.Count < 3)
                return null;

            return new HomeInsightDto
            {
                Key = "community-mood",
                Title = "Community mood",
                Subtitle = $"Average rating {stats.Avg:F2} based on {stats.Count} recent reviews",
                Icon = InsightIconEnum.ChartLine,
                Scope = InsightScopeEnum.Global
            };
        }
    }
}
