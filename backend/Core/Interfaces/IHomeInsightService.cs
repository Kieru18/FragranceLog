using Core.DTOs;

namespace Core.Interfaces
{
    public interface IHomeInsightService
    {
        Task<IReadOnlyList<HomeInsightDto>> GetInsightsAsync(int userId, CancellationToken ct);
    }
}
