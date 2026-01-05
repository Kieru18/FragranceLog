using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class BrandMomentumInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public BrandMomentumInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Global;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddHours(-48);

            var result = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReviewDate >= since)
                .GroupBy(r => r.Perfume.BrandId)
                .Select(g => new
                {
                    BrandId = g.Key,
                    Count = g.Count()
                })
                .Where(x => x.Count >= 5)
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

            if (result == null)
                return null;

            var brand = await _context.Brands
                .Where(b => b.BrandId == result.BrandId)
                .Select(b => b.Name)
                .SingleAsync(ct);

            return new HomeInsightDto
            {
                Key = "brand-momentum",
                Title = "Brand momentum",
                Subtitle = $"{brand} is gaining attention with {result.Count} new reviews",
                Icon = InsightIconEnum.Building,
                Scope = InsightScopeEnum.Global
            };
        }
    }
}
