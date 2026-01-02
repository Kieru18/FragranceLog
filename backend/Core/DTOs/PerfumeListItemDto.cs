namespace Core.DTOs;

public sealed class PerfumeListItemDto
{
    public int PerfumeId { get; init; }
    public string Name { get; init; } = null!;
    public string Brand { get; init; } = null!;
    public string? ImageUrl { get; init; }

    public double AvgRating { get; init; }
    public int RatingCount { get; init; }

    public int? MyRating { get; init; }
}
