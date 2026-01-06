using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class TrendingPerfumeInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public TrendingPerfumeInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Global;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddHours(-24);

            var data = await _context.Reviews
                .Where(r => r.ReviewDate >= since)
                .GroupBy(r => r.Perfume)
                .OrderByDescending(g => g.Count())
                .Select(g => new
                {
                    Perfume = g.Key,
                    Count = g.Count()
                })
                .FirstOrDefaultAsync(ct);

            if (data == null || data.Count < 3)
                return null;

            return new HomeInsightDto
            {
                Key = "trending-perfume",
                Title = "Trending now",
                Subtitle = $"{data.Perfume.Name} received {data.Count} reviews today",
                Icon = InsightIconEnum.Flame,
                Scope = InsightScopeEnum.Global
            };
        }
    }
}
