using Core.Entities;
using Core.Enums;

namespace Infrastructure.Tests.Builders;

internal sealed class PerfumeLongevityVoteBuilder
{
    private int _perfumeId = 1;
    private int _userId = 1;
    private int _longevityId = 3;

    public static PerfumeLongevityVoteBuilder Default() => new();

    public PerfumeLongevityVoteBuilder For(Perfume perfume, User user)
    {
        _perfumeId = perfume.PerfumeId;
        _userId = user.UserId;
        return this;
    }

    public PerfumeLongevityVoteBuilder WithLongevity(int longevityId)
    {
        _longevityId = longevityId;
        return this;
    }

    public static PerfumeLongevityVote For(
        Perfume perfume,
        User user,
        LongevityEnum longevity)
    {
        return new PerfumeLongevityVote
        {
            PerfumeId = perfume.PerfumeId,
            UserId = user.UserId,
            LongevityId = (int)longevity
        };
    }

    public PerfumeLongevityVote Build()
        => new()
        {
            PerfumeId = _perfumeId,
            UserId = _userId,
            LongevityId = _longevityId
        };
}
