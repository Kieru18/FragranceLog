using Core.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class FavoriteBrandInsightProviderTests
{
    private static FavoriteBrandInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_user_has_less_than_three_reviews_for_any_brand()
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
            ReviewBuilder.Default().For(perfume, user).WithRating(5).Build(),
            ReviewBuilder.Default().For(perfume, user).WithRating(4).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_for_most_reviewed_brand()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();

        var brandA = BrandBuilder.Default().WithId(1).Build();
        var brandB = BrandBuilder.Default().WithId(2).Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brandA).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brandA).Build();
        var p3 = PerfumeBuilder.Default().WithId(3).WithBrand(brandA).Build();
        var pOther = PerfumeBuilder.Default().WithId(4).WithBrand(brandB).Build();

        ctx.AddRange(user, brandA, brandB, p1, p2, p3, pOther);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, user).Build(),
            ReviewBuilder.Default().For(p2, user).Build(),
            ReviewBuilder.Default().For(p3, user).Build(),
            ReviewBuilder.Default().For(pOther, user).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("favorite-brand");
        result.Scope.Should().Be(InsightScopeEnum.Personal);
        result.Icon.Should().Be(InsightIconEnum.Heart);
        result.Subtitle.Should().Contain(brandA.Name);
    }
}
