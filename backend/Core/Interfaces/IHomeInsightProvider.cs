using Core.DTOs;
using Core.Enums;

namespace Core.Interfaces
{
    public interface IHomeInsightProvider
    {
        InsightScopeEnum Scope { get; }
        Task<HomeInsightDto?> TryBuildAsync(int userId, CancellationToken ct);
    }
}
