using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class TopRatedLast24hInsightProviderTests
{
    private static TopRatedLast24hInsightProvider CreateSut(
        FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_no_recent_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var result = await CreateSut(ctx)
            .TryBuildAsync(userId: 1, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_null_when_review_count_below_threshold()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();

        var perfume = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .Build();

        ctx.AddRange(user, brand, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(5)
                .WithDate(DateTime.UtcNow)
                .Build(),
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(4)
                .WithDate(DateTime.UtcNow)
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_top_rated_perfume_from_last_24h()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var u3 = UserBuilder.Default().WithId(3).Build();

        var brand = BrandBuilder.Default().Build();

        var p1 = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithName("Alpha")
            .Build();

        var p2 = PerfumeBuilder.Default()
            .WithId(2)
            .WithBrand(brand)
            .WithName("Beta")
            .Build();

        ctx.AddRange(u1, u2, u3, brand, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, u1).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p1, u2).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p1, u3).WithRating(4).WithDate(DateTime.UtcNow).Build(),

            ReviewBuilder.Default().For(p2, u1).WithRating(4).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p2, u2).WithRating(4).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p2, u3).WithRating(4).WithDate(DateTime.UtcNow).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(u1.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("top-rated-24h");
        result.Subtitle.Should().Contain("Alpha");
    }
}
