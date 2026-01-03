namespace Core.DTOs
{
    public sealed class ChangePasswordDto
    {
        public string CurrentPassword { get; init; } = null!;
        public string NewPassword { get; init; } = null!;
    }
}
