using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class PersonalReviewActivityInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public PersonalReviewActivityInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Personal;

        public async Task<HomeInsightDto?> TryBuildAsync(
            int userId,
            CancellationToken ct)
        {
            var since = DateTime.UtcNow.AddDays(-7);

            var count = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.ReviewDate >= since)
                .CountAsync(ct);

            if (count == 0)
                return null;

            return new HomeInsightDto
            {
                Key = "your-activity",
                Title = "Your activity",
                Subtitle = $"You added {count} reviews in the last 7 days",
                Icon = InsightIconEnum.User,
                Scope = InsightScopeEnum.Personal
            };
        }
    }

}
