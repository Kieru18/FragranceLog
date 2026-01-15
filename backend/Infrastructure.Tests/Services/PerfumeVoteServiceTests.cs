using Core.Entities;
using Core.Enums;
using Core.Services;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class PerfumeVoteServiceTests
{
    private static PerfumeVoteService CreateSut(FragranceLogContext ctx)
        => new(ctx);

    private static async Task<(FragranceLogContext ctx, SqliteConnection conn, Perfume perfume, User user)>
    CreatePerfumeAndUserAsync()
    {
        var (ctx, conn) = DbContextFactory.Create();

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();
        var user = UserBuilder.Default().Build();

        ctx.AddRange(brand, perfume, user);
        await ctx.SaveChangesAsync();

        return (ctx, conn, perfume, user);
    }


    [Fact]
    public async Task SetGenderVoteAsync_creates_vote_when_not_exists()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetGenderVoteAsync(
            perfume.PerfumeId,
            user.UserId,
            GenderEnum.Male);

        ctx.PerfumeGenderVotes.Should().ContainSingle(v =>
            v.PerfumeId == perfume.PerfumeId &&
            v.UserId == user.UserId &&
            v.GenderId == (int)GenderEnum.Male);
    }

    [Fact]
    public async Task SetGenderVoteAsync_updates_existing_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetGenderVoteAsync(perfume.PerfumeId, user.UserId, GenderEnum.Male);
        await CreateSut(ctx).SetGenderVoteAsync(perfume.PerfumeId, user.UserId, GenderEnum.Unisex);

        ctx.PerfumeGenderVotes.Should().ContainSingle(v =>
            v.GenderId == (int)GenderEnum.Unisex);
    }

    [Fact]
    public async Task SetGenderVoteAsync_deletes_vote_when_null()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetGenderVoteAsync(perfume.PerfumeId, user.UserId, GenderEnum.Male);
        await CreateSut(ctx).SetGenderVoteAsync(perfume.PerfumeId, user.UserId, null);

        ctx.PerfumeGenderVotes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteGenderVoteAsync_removes_existing_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetGenderVoteAsync(perfume.PerfumeId, user.UserId, GenderEnum.Male);
        await CreateSut(ctx).DeleteGenderVoteAsync(perfume.PerfumeId, user.UserId, default);

        ctx.PerfumeGenderVotes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteGenderVoteAsync_is_noop_when_vote_missing()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx)
            .Invoking(x => x.DeleteGenderVoteAsync(perfume.PerfumeId, user.UserId, default))
            .Should()
            .NotThrowAsync();

        ctx.PerfumeGenderVotes.Should().BeEmpty();
    }

    [Fact]
    public async Task SetSillageVoteAsync_creates_and_deletes_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetSillageVoteAsync(perfume.PerfumeId, user.UserId, SillageEnum.Strong);

        ctx.PerfumeSillageVotes.Should().ContainSingle();

        await CreateSut(ctx).SetSillageVoteAsync(perfume.PerfumeId, user.UserId, null);

        ctx.PerfumeSillageVotes.Should().BeEmpty();
    }

    [Fact]
    public async Task SetLongevityVoteAsync_creates_and_updates_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetLongevityVoteAsync(perfume.PerfumeId, user.UserId, LongevityEnum.Moderate);
        await CreateSut(ctx).SetLongevityVoteAsync(perfume.PerfumeId, user.UserId, LongevityEnum.LongLasting);

        ctx.PerfumeLongevityVotes.Should().ContainSingle(v =>
            v.LongevityId == (int)LongevityEnum.LongLasting);
    }

    [Fact]
    public async Task SetSeasonVoteAsync_creates_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetSeasonVoteAsync(perfume.PerfumeId, user.UserId, SeasonEnum.Summer);

        ctx.PerfumeSeasonVotes.Should().ContainSingle();
    }

    [Fact]
    public async Task SetDaytimeVoteAsync_creates_vote()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).SetDaytimeVoteAsync(perfume.PerfumeId, user.UserId, DaytimeEnum.Day);

        ctx.PerfumeDaytimeVotes.Should().ContainSingle();
    }
}
