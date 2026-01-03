namespace Core.DTOs
{
    public sealed class DiscordEmbed
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public int? Color { get; init; }
        public IReadOnlyList<DiscordEmbedField>? Fields { get; init; }
        public DiscordEmbedImage? Image { get; init; }
        public DiscordEmbedFooter? Footer { get; init; }
        public string? Timestamp { get; init; }
    }
}
