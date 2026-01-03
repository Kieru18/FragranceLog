namespace Core.DTOs
{
    public sealed class DiscordWebhookPayload
    {
        public string? Username { get; init; }
        public IReadOnlyList<DiscordEmbed> Embeds { get; init; } = [];
    }
}
