namespace Core.DTOs
{
    public sealed class UserProfileDto
    {
        public int Id { get; init; }
        public string DisplayName { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
}
