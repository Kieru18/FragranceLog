namespace Core.DTOs
{
    public sealed class PerfumeSearchResponseDto
    {
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public IReadOnlyList<PerfumeSearchResultDto> Items { get; init; } = [];
    }
}
