using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class TopRatedLast24hInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public TopRatedLast24hInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Global;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddHours(-24);

            var result = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewDate >= since)
                .GroupBy(r => r.PerfumeId)
                .Select(g => new
                {
                    PerfumeId = g.Key,
                    Avg = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .Where(x => x.Count >= 3)
                .OrderByDescending(x => x.Avg)
                .FirstOrDefaultAsync(ct);

            if (result == null)
                return null;

            var perfume = await _context.Perfumes
                .Where(p => p.PerfumeId == result.PerfumeId)
                .Select(p => p.Name)
                .SingleAsync(ct);

            return new HomeInsightDto
            {
                Key = "top-rated-24h",
                Title = "Top rated today",
                Subtitle = $"{perfume} averages {result.Avg:F2} from {result.Count} reviews",
                Icon = InsightIconEnum.Star,
                Scope = InsightScopeEnum.Global
            };
        }
    }
}
