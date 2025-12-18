using Core.Enums;

namespace Core.DTOs
{
    public sealed class PerfumeNoteDto
    {
        public int NoteId { get; init; }
        public string Name { get; init; } = null!;
        public NoteTypeEnum Type { get; init; }
    }
}
