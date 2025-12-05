namespace Core.DTOs
{
    public sealed class PerfumeSearchRequestDto
    {
        public string Query { get; init; } = "";

        public int? BrandId { get; init; }
        public string? CountryCode { get; init; }
        public double? MinRating { get; init; }

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 25;
    }
}
