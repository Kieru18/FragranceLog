using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class PersonalRatingBiasInsightProviderTests
{
    private static PersonalRatingBiasInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_not_enough_ratings()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 4)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).Build())
            .ToList();

        ctx.AddRange(user, brand);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
            ctx.Add(ReviewBuilder.Default().For(p, user).WithRating(4).Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_generous_bias_when_average_high()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 5)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).Build())
            .ToList();

        ctx.AddRange(user, brand);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
            ctx.Add(ReviewBuilder.Default().For(p, user).WithRating(5).Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("rating-bias");
        result.Subtitle.Should().Contain("generously");
    }

    [Fact]
    public async Task TryBuildAsync_returns_strict_bias_when_average_low()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 5)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).Build())
            .ToList();

        ctx.AddRange(user, brand);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
            ctx.Add(ReviewBuilder.Default().For(p, user).WithRating(2).Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("rating-bias");
        result.Subtitle.Should().Contain("strict");
    }
}
