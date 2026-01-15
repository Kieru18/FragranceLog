using Core.Enums;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class BrandMomentumInsightProviderTests
{
    private static BrandMomentumInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_no_brand_meets_threshold()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();
        var user = UserBuilder.Default().Build();

        ctx.AddRange(brand, perfume, user);
        await ctx.SaveChangesAsync();

        ctx.Add(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithDate(DateTime.UtcNow)
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_when_brand_has_enough_recent_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().WithName("TestBrand").Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(brand, perfume);
        await ctx.SaveChangesAsync();

        for (var i = 1; i <= 5; i++)
        {
            var user = UserBuilder.Default().WithId(i).Build();
            ctx.Add(user);

            ctx.Add(
                ReviewBuilder.Default()
                    .For(perfume, user)
                    .WithDate(DateTime.UtcNow)
                    .Build()
            );
        }

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(1, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("brand-momentum");
        result.Scope.Should().Be(InsightScopeEnum.Global);
        result.Subtitle.Should().Contain("TestBrand");
        result.Icon.Should().Be(InsightIconEnum.Building);
    }
}
