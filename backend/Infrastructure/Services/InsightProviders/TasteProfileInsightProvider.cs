using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.InsightProviders
{
    public sealed class TasteProfileInsightProvider : IHomeInsightProvider
    {
        private readonly FragranceLogContext _context;

        public TasteProfileInsightProvider(FragranceLogContext context)
        {
            _context = context;
        }

        public InsightScopeEnum Scope => InsightScopeEnum.Personal;

        public async Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct)
        {
            var result = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .SelectMany(r => r.Perfume.Groups)
                .GroupBy(g => g.Name)
                .Select(g => new { Group = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(ct);


            if (result == null || result.Count < 3)
                return null;

            return new HomeInsightDto
            {
                Key = "taste-profile",
                Title = "Your taste profile",
                Subtitle = $"You gravitate toward {result.Group} fragrances",
                Icon = InsightIconEnum.Flask,
                Scope = InsightScopeEnum.Personal
            };
        }
    }
}
