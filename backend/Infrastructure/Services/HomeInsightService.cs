using Core.DTOs;
using Core.Enums;
using Core.Interfaces;

namespace Infrastructure.Services
{
    public sealed class HomeInsightService : IHomeInsightService
    {
        private readonly IReadOnlyList<IHomeInsightProvider> _providers;

        public HomeInsightService(IEnumerable<IHomeInsightProvider> providers)
        {
            _providers = providers.ToList();
        }

        public async Task<IReadOnlyList<HomeInsightDto>> GetInsightsAsync(
            int userId,
            CancellationToken ct)
        {
            var allInsights = new List<HomeInsightDto>();

            foreach (var provider in _providers)
            {
                var insight = await provider.TryBuildAsync(userId, ct);
                if (insight != null)
                    allInsights.Add(insight);
            }

            var global = PickRandom(
                allInsights.Where(i => i.Scope == InsightScopeEnum.Global),
                max: 2);

            var personal = PickRandom(
                allInsights.Where(i => i.Scope == InsightScopeEnum.Personal),
                max: 2);

            return global.Concat(personal).ToList();
        }

        private static List<HomeInsightDto> PickRandom(
            IEnumerable<HomeInsightDto> source,
            int max)
        {
            return source
                .OrderBy(_ => Random.Shared.Next())
                .Take(max)
                .ToList();
        }
    }
}
