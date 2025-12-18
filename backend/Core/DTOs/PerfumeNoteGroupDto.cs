using Core.Enums;

namespace Core.DTOs
{
    public sealed class PerfumeNoteGroupDto
    {
        public NoteTypeEnum Type { get; init; }
        public IReadOnlyList<PerfumeNoteDto> Notes { get; init; } = [];
    }
}
