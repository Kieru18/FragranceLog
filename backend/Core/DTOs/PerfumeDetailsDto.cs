namespace Core.DTOs
{
    public sealed class PerfumeDetailsDto
    {
        public int PerfumeId { get; init; }
        public string Name { get; init; } = null!;
        public string Brand { get; init; } = null!;

        public string? ImageUrl { get; init; }

        public double AvgRating { get; init; }
        public int RatingCount { get; init; }

        public string? Gender { get; init; }
        public string? Longevity { get; init; }
        public string? Sillage { get; init; }
        public IReadOnlyList<string> Seasons { get; init; } = [];
        public IReadOnlyList<string> Daytimes { get; init; } = [];

        public int? MyRating { get; init; }
        public string? MyReview { get; init; }
        public string? MyGenderVote { get; init; }
        public string? MyLongevityVote { get; init; }
        public string? MySillageVote { get; init; }

        public IReadOnlyList<string> Groups { get; init; } = [];
        public IReadOnlyList<PerfumeNoteDto> Notes { get; init; } = [];

        public IReadOnlyList<ReviewDto> Reviews { get; init; } = [];
    }

}
