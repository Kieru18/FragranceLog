using Core.Entities;
using Core.Enums;

namespace Infrastructure.Tests.Builders;

internal sealed class PerfumeGenderVoteBuilder
{
    private int _perfumeId = 1;
    private int _userId = 1;
    private int _genderId = 3;

    public static PerfumeGenderVoteBuilder Default() => new();

    public PerfumeGenderVoteBuilder For(Perfume perfume, User user)
    {
        _perfumeId = perfume.PerfumeId;
        _userId = user.UserId;
        return this;
    }

    public PerfumeGenderVoteBuilder WithGender(int genderId)
    {
        _genderId = genderId;
        return this;
    }

    public static PerfumeGenderVote For(
        Perfume perfume,
        User user,
        GenderEnum gender)
    {
        return new PerfumeGenderVote
        {
            PerfumeId = perfume.PerfumeId,
            UserId = user.UserId,
            GenderId = (int)gender
        };
    }

    public PerfumeGenderVote Build()
        => new()
        {
            PerfumeId = _perfumeId,
            UserId = _userId,
            GenderId = _genderId
        };
}
