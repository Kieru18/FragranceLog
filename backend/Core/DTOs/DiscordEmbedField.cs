namespace Core.DTOs
{
    public sealed class DiscordEmbedField
    {
        public string Name { get; init; } = null!;
        public string Value { get; init; } = null!;
        public bool Inline { get; init; }
    }
}
