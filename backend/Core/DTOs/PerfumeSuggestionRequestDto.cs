namespace Core.DTOs
{
    public sealed class PerfumeSuggestionRequestDto
    {
        public string Brand { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Comment { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageBase64 { get; init; }
        public IReadOnlyList<string> Groups { get; init; } = [];
        public IReadOnlyList<NoteGroupDto> NoteGroups { get; init; } = [];
    }
}
