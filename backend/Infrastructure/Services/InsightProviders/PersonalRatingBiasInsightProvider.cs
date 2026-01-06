using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class PersonalRatingBiasInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public PersonalRatingBiasInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Personal;

        public async Task<HomeInsightDto?> TryBuildAsync(
            int userId,
            CancellationToken ct)
        {
            var ratings = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.Rating)
                .ToListAsync(ct);

            if (ratings.Count < 5)
                return null;

            var avg = ratings.Average();

            return new HomeInsightDto
            {
                Key = "rating-bias",
                Title = "Your rating style",
                Subtitle = avg >= 4.5
                    ? "You tend to rate perfumes generously"
                    : avg <= 3.0
                        ? "You are a strict reviewer"
                        : "Your ratings are well balanced",
                Icon = InsightIconEnum.BalanceScale,
                Scope = InsightScopeEnum.Personal
            };
        }
    }
}
