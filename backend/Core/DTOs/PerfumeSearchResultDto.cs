namespace Core.DTOs
{
    public sealed class PerfumeSearchResultDto
    {
        public int PerfumeId { get; init; }
        public string Name { get; init; } = null!;
        public string Brand { get; init; } = null!;
        public string Country { get; init; } = null!;

        public double Rating { get; init; }
        public int RatingCount { get; init; }
    }

}
