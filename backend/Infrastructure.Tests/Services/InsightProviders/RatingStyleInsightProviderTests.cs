using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class RatingStyleInsightProviderTests
{
    private static RatingStyleInsightProvider CreateSut(
        FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_user_has_no_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_result_when_only_user_reviews_exist()
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

        ctx.Add(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(5)
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("rating-style");
    }

    [Fact]
    public async Task TryBuildAsync_returns_higher_style_when_user_rates_higher_than_global()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var other = UserBuilder.Default().WithId(user.UserId + 1).Build();
        var brand = BrandBuilder.Default().Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(user, other, brand, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, user).WithRating(5).Build(),
            ReviewBuilder.Default().For(p2, other).WithRating(3).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("rating-style");
        result.Subtitle.Should().Contain("higher");
    }

    [Fact]
    public async Task TryBuildAsync_returns_lower_style_when_user_rates_lower_than_global()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var other = UserBuilder.Default().WithId(user.UserId + 1).Build();
        var brand = BrandBuilder.Default().Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(user, other, brand, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, user).WithRating(2).Build(),
            ReviewBuilder.Default().For(p2, other).WithRating(5).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Subtitle.Should().Contain("lower");
    }
}
