using Core.Enums;

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
        public int CommentCount { get; init; }

        public GenderEnum? Gender { get; init; }
        public SeasonEnum? Season { get; init; }
        public DaytimeEnum? Daytime { get; init; }
        public double? Longevity { get; init; }
        public double? Sillage { get; init; }

        public int? MyRating { get; init; }
        public string? MyReview { get; init; }

        public GenderEnum? MyGenderVote { get; init; }
        public SeasonEnum? MySeasonVote { get; init; }
        public DaytimeEnum? MyDaytimeVote { get; init; }
        public LongevityEnum? MyLongevityVote { get; init; }
        public SillageEnum? MySillageVote { get; init; }

        public IReadOnlyList<string> Groups { get; init; } = [];
        public IReadOnlyList<PerfumeNoteGroupDto> NoteGroups { get; init; } = [];

        public IReadOnlyList<ReviewDto> Reviews { get; init; } = [];
    }
}
