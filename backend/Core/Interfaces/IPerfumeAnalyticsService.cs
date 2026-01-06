using Core.DTOs;

namespace Core.Interfaces
{
    public interface IPerfumeAnalyticsService
    {
        Task<PerfumeOfTheDayDto?> GetPerfumeOfTheDayAsync();
        Task<IReadOnlyList<HomeRecentReviewDto>> GetRecentReviewsAsync(int take, CancellationToken ct);
        Task<HomeStatsDto> GetStatsAsync(CancellationToken ct);
        Task<IReadOnlyList<HomeCountryPerfumeDto>> GetTopFromCountryAsync(
            double lat,
            double lng,
            int take,
            CancellationToken ct);
    }
}
