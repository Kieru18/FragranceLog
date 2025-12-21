namespace Core.DTOs;

public sealed class PerfumeListMembershipDto
{
    public int PerfumeListId { get; init; }
    public string Name { get; init; } = null!;
    public bool IsSystem { get; init; }
    public bool ContainsPerfume { get; init; }
}
