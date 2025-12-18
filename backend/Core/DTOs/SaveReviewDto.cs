namespace Core.DTOs
{
    public sealed class SaveReviewDto
    {
        public int PerfumeId { get; init; }
        public int Rating { get; init; }
        public string? Text { get; init; }
    }
}
