using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class GlobalTasteInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public GlobalTasteInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Global;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-7);

            var result = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewDate >= since)
                .SelectMany(r => r.Perfume.Groups)
                .GroupBy(g => g.Name)
                .Select(g => new
                {
                    GroupName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

            if (result == null || result.Count < 10)
                return null;

            return new HomeInsightDto
            {
                Key = "global-taste",
                Title = "Current taste trend",
                Subtitle = $"The community leans toward {result.GroupName} fragrances",
                Icon = InsightIconEnum.Compass,
                Scope = InsightScopeEnum.Global
            };
        }
    }
}
