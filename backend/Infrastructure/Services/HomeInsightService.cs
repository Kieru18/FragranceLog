using Core.DTOs;
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
            var insights = new List<HomeInsightDto>();

            foreach (var provider in _providers)
            {
                var insight = await provider.TryBuildAsync(userId, ct);
                if (insight != null)
                    insights.Add(insight);
            }

            return insights;
        }
    }
}
