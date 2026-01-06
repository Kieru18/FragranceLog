namespace Core.DTOs
{
    public sealed class HomeRecentReviewDto
    {
        public int ReviewId { get; init; }
        public int PerfumeId { get; init; }
        public string PerfumeName { get; init; } = null!;
        public string Brand { get; init; } = null!;
        public string? PerfumeImageUrl { get; init; }
        public string Author { get; init; } = null!;
        public int Rating { get; init; }
        public string Comment { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
    }
}
