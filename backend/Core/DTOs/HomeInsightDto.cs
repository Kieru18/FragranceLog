using Core.Enums;

namespace Core.DTOs
{
    public sealed class HomeInsightDto
    {
        public string Key { get; init; } = null!;
        public string Title { get; init; } = null!;
        public string Subtitle { get; init; } = null!;
        public InsightIconEnum Icon { get; init; }
        public InsightScopeEnum Scope { get; init; }
    }
}
