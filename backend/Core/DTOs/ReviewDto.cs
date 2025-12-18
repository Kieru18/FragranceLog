namespace Core.DTOs
{
    public sealed class ReviewDto
    {
        public int ReviewId { get; init; }
        public string Author { get; init; } = null!;
        public int Rating { get; init; }
        public string? Text { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
