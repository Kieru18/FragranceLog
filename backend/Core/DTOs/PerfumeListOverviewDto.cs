namespace Core.DTOs;

public sealed class PerfumeListOverviewDto
{
    public int PerfumeListId { get; init; }
    public string Name { get; init; } = null!;
    public bool IsSystem { get; init; }

    public int PerfumeCount { get; init; }
    public IReadOnlyList<string> PreviewImages { get; init; } = [];
}
