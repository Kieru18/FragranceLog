using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class PersonalReviewActivityInsightProviderTests
{
    private static PersonalReviewActivityInsightProvider CreateSut(
        FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_user_has_no_recent_reviews()
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
                .WithDate(DateTime.UtcNow.AddDays(-10))
                .Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_when_user_has_recent_reviews()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 3)
            .Select(i => PerfumeBuilder.Default()
                .WithId(i)
                .WithBrand(brand)
                .Build())
            .ToList();

        ctx.AddRange(user, brand);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
        {
            ctx.Add(
                ReviewBuilder.Default()
                    .For(p, user)
                    .WithDate(DateTime.UtcNow.AddDays(-2))
                    .Build()
            );
        }

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("your-activity");
        result.Subtitle.Should().Contain("3");
    }
}
