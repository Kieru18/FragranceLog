using Core.Entities;

namespace Infrastructure.Tests.Builders;

internal sealed class PerfumeSillageVoteBuilder
{
    private int _perfumeId = 1;
    private int _userId = 1;
    private int _sillageId = 2;

    public static PerfumeSillageVoteBuilder Default() => new();

    public PerfumeSillageVoteBuilder For(Perfume perfume, User user)
    {
        _perfumeId = perfume.PerfumeId;
        _userId = user.UserId;
        return this;
    }

    public PerfumeSillageVoteBuilder WithSillage(int sillageId)
    {
        _sillageId = sillageId;
        return this;
    }

    public PerfumeSillageVote Build()
        => new()
        {
            PerfumeId = _perfumeId,
            UserId = _userId,
            SillageId = _sillageId
        };
}
