using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class TrendingPerfumeInsightProviderTests
{
    private static TrendingPerfumeInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_no_recent_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var result = await CreateSut(ctx).TryBuildAsync(userId: 1, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_null_when_top_perfume_has_less_than_3_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var brand = BrandBuilder.Default().Build();

        var perfume = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithName("Alpha")
            .Build();

        ctx.AddRange(u1, u2, brand, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(perfume, u1).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(perfume, u2).WithRating(4).WithDate(DateTime.UtcNow).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).TryBuildAsync(u1.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_for_most_reviewed_perfume_in_last_24h()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var u3 = UserBuilder.Default().WithId(3).Build();
        var u4 = UserBuilder.Default().WithId(4).Build();

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

        ctx.AddRange(u1, u2, u3, u4, brand, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, u1).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p1, u2).WithRating(4).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p1, u3).WithRating(4).WithDate(DateTime.UtcNow).Build(),

            ReviewBuilder.Default().For(p2, u1).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p2, u2).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p2, u3).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p2, u4).WithRating(5).WithDate(DateTime.UtcNow).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).TryBuildAsync(u1.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("trending-perfume");
        result.Title.Should().Be("Trending now");
        result.Subtitle.Should().Contain("Beta");
        result.Subtitle.Should().Contain("4");
    }

    [Fact]
    public async Task TryBuildAsync_ignores_reviews_older_than_24_hours()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var u3 = UserBuilder.Default().WithId(3).Build();
        var u4 = UserBuilder.Default().WithId(4).Build();

        var brand = BrandBuilder.Default().Build();

        var oldPopular = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithName("Old King")
            .Build();

        var recentTrending = PerfumeBuilder.Default()
            .WithId(2)
            .WithBrand(brand)
            .WithName("New Star")
            .Build();

        ctx.AddRange(u1, u2, u3, u4, brand, oldPopular, recentTrending);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(oldPopular, u1).WithRating(5).WithDate(DateTime.UtcNow.AddDays(-2)).Build(),
            ReviewBuilder.Default().For(oldPopular, u2).WithRating(5).WithDate(DateTime.UtcNow.AddDays(-2)).Build(),
            ReviewBuilder.Default().For(oldPopular, u3).WithRating(5).WithDate(DateTime.UtcNow.AddDays(-2)).Build(),
            ReviewBuilder.Default().For(oldPopular, u4).WithRating(5).WithDate(DateTime.UtcNow.AddDays(-2)).Build(),

            ReviewBuilder.Default().For(recentTrending, u1).WithRating(5).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(recentTrending, u2).WithRating(4).WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(recentTrending, u3).WithRating(4).WithDate(DateTime.UtcNow).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).TryBuildAsync(u1.UserId, default);

        result.Should().NotBeNull();
        result!.Subtitle.Should().Contain("New Star");
        result.Subtitle.Should().Contain("3");
    }
}
