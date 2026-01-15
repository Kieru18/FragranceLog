using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services.InsightProviders;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services.InsightProviders;

public sealed class TasteProfileInsightProviderTests
{
    private static TasteProfileInsightProvider CreateSut(
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
    public async Task TryBuildAsync_returns_null_when_group_count_below_threshold()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var group = GroupBuilder.Default().WithId(1).WithName("Woody").Build();

        var perfume = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithGroups(group)
            .Build();

        ctx.AddRange(user, brand, group, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(perfume, user).Build(),
            ReviewBuilder.Default().For(perfume, user).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryBuildAsync_returns_taste_profile_when_group_is_dominant()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var group = GroupBuilder.Default().WithId(1).WithName("Woody").Build();

        var perfume = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithGroups(group)
            .Build();

        ctx.AddRange(user, brand, group, perfume);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(perfume, user).Build(),
            ReviewBuilder.Default().For(perfume, user).Build(),
            ReviewBuilder.Default().For(perfume, user).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx)
            .TryBuildAsync(user.UserId, default);

        result.Should().NotBeNull();
        result!.Key.Should().Be("taste-profile");
        result.Subtitle.Should().Contain("Woody");
    }
}
