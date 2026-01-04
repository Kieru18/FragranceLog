using Core.Enums;

namespace Core.DTOs
{
    public sealed class PerfumeOfTheDayDto
    {
        public int PerfumeId { get; set; }
        public string Name { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public double AvgRating { get; set; }
        public int RatingCount { get; set; }
        public double Score { get; set; }
        public PerfumeHighlightType Type { get; set; }
        public string Reason { get; set; } = null!;
    }
}
