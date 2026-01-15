using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class GlobalTasteInsightProviderTests
{
    private static GlobalTasteInsightProvider CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task TryBuildAsync_returns_null_when_group_count_below_threshold()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var group = GroupBuilder.Default().WithName("Woody").Build();
        var user = UserBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 3)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).WithGroups(group).Build())
            .ToList();

        ctx.AddRange(user, brand, group);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
            ctx.Add(ReviewBuilder.Default().For(p, user).Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_insight_for_dominant_group()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var group = GroupBuilder.Default().WithName("Amber").Build();
        var user = UserBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 10)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).WithGroups(group).Build())
            .ToList();

        ctx.AddRange(user, brand, group);
        ctx.AddRange(perfumes);
        await ctx.SaveChangesAsync();

        foreach (var p in perfumes)
            ctx.Add(ReviewBuilder.Default().For(p, user).Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("global-taste");
        result.Subtitle.Should().Contain("Amber");
    }
}
