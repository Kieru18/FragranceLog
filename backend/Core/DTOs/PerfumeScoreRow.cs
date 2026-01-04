namespace Core.DTOs
{
    public sealed class PerfumeScoreRow
    {
        public int PerfumeId { get; init; }
        public double Score { get; init; }
        public double AvgRating { get; init; }
        public int RatingCount { get; init; }
    }
}
