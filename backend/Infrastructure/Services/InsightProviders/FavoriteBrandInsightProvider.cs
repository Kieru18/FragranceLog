using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class FavoriteBrandInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public FavoriteBrandInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Personal;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var result = await _context.Reviews
                .Where(r => r.UserId == userId)
                .GroupBy(r => r.Perfume.BrandId)
                .Select(g => new
                {
                    BrandId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);

            if (result == null || result.Count < 3)
                return null;

            var brand = await _context.Brands
                .Where(b => b.BrandId == result.BrandId)
                .Select(b => b.Name)
                .SingleAsync(ct);

            return new HomeInsightDto
            {
                Key = "favorite-brand",
                Title = "Your favorite brand",
                Subtitle = $"You review {brand} more than any other brand",
                Icon = InsightIconEnum.Heart,
                Scope = InsightScopeEnum.Personal
            };
        }
    }
}
