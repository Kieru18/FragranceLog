namespace Core.DTOs
{
    public sealed class SharedListPerfumePreviewDto
    {
        public int PerfumeId { get; set; }
        public string Name { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public double? AvgRating { get; set; }
        public int RatingCount { get; set; }
        public string? ImageUrl { get; set; }
    }
}
