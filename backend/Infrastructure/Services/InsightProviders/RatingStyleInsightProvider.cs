using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class RatingStyleInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public RatingStyleInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Personal;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var userAvg = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => (double?)r.Rating)
                .AverageAsync(ct);

            if (userAvg == null)
                return null;

            var globalAvg = await _context.Reviews
                .Select(r => (double?)r.Rating)
                .AverageAsync(ct);

            if (globalAvg == null)
                return null;

            var style = userAvg > globalAvg ? "higher" : "lower";

            return new HomeInsightDto
            {
                Key = "rating-style",
                Title = "Your rating style",
                Subtitle = $"You rate perfumes {style} than the community average",
                Icon = InsightIconEnum.Sliders,
                Scope = InsightScopeEnum.Personal
            };
        }
    }
}
