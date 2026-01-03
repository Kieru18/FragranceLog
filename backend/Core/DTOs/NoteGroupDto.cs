using Core.Enums;

namespace Core.DTOs
{
    public sealed class NoteGroupDto
    {
        public NoteTypeEnum Type { get; init; }
        public IReadOnlyList<string> Notes { get; init; } = [];
    }
}
