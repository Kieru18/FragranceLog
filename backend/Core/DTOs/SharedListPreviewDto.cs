namespace Core.DTOs
{
    public sealed class SharedListPreviewDto
    {
        public Guid ShareToken { get; set; }
        public string ListName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public int PerfumeCount { get; set; }
        public IReadOnlyList<string> PreviewImages { get; set; } = [];
    }
}
