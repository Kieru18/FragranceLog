namespace Core.DTOs
{
    public sealed class UpdateProfileDto
    {
        public string DisplayName { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
}
