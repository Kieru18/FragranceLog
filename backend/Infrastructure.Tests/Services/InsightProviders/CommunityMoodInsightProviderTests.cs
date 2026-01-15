using Core.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class CommunityMoodInsightProviderTests
{
    private static CommunityMoodInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_not_enough_recent_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(user, brand, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(5)
                .WithDate(DateTime.UtcNow.AddHours(-1))
                .Build(),
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(4)
                .WithDate(DateTime.UtcNow.AddHours(-2))
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_when_threshold_is_met()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(user, brand, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(5)
                .WithDate(DateTime.UtcNow.AddHours(-1))
                .Build(),
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(4)
                .WithDate(DateTime.UtcNow.AddHours(-2))
                .Build(),
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(3)
                .WithDate(DateTime.UtcNow.AddHours(-3))
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("community-mood");
        result.Scope.Should().Be(InsightScopeEnum.Global);
        result.Icon.Should().Be(InsightIconEnum.ChartLine);
        result.Subtitle.Should().Contain("based on 3 recent reviews");
    }
}
