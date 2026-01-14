using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Services;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class PerfumeAnalyticsServiceTests
{
    private readonly Mock<IGeoService> _geo = new();

    private PerfumeAnalyticsService CreateSut(FragranceLogContext ctx)
        => new(ctx, _geo.Object);

    [Fact]
    public async Task GetStatsAsync_returns_correct_counts()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(brand, u1, u2, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, u1).Build(),
            ReviewBuilder.Default().For(p2, u2).Build()
        );
        await ctx.SaveChangesAsync();

        var stats = await CreateSut(ctx).GetStatsAsync(default);

        stats.Perfumes.Should().Be(2);
        stats.Users.Should().Be(2);
        stats.Reviews.Should().Be(2);
    }

    [Fact]
    public async Task GetRecentReviews_filters_empty_comments_and_orders()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var user = UserBuilder.Default().WithId(1).Build();
        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(brand, user, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p1, user).WithComment("Newest").WithDate(DateTime.UtcNow).Build(),
            ReviewBuilder.Default().For(p1, user).WithComment(null).Build(),
            ReviewBuilder.Default().For(p2, user).WithComment(" ").Build(),
            ReviewBuilder.Default().For(p2, user).WithComment("Older").WithDate(DateTime.UtcNow.AddMinutes(-10)).Build()
        );
        await ctx.SaveChangesAsync();

        var rows = await CreateSut(ctx).GetRecentReviewsAsync(2, default);

        rows.Should().HaveCount(2);
        rows[0].Comment.Should().Be("Newest");
        rows[1].Comment.Should().Be("Older");
    }

    [Fact]
    public async Task GetTopFromCountry_returns_only_mapped_country_and_sorted()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var users = UserBuilder.Many(3).ToList();

        var pol1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").Build();
        var pol2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithCountry("POL").Build();
        var fra = PerfumeBuilder.Default().WithId(3).WithBrand(brand).WithCountry("FRA").Build();

        ctx.AddRange(brand);
        ctx.AddRange(users);
        ctx.AddRange(pol1, pol2, fra);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(pol1, users[0]).WithRating(4).Build(),
            ReviewBuilder.Default().For(pol1, users[1]).WithRating(5).Build(),
            ReviewBuilder.Default().For(pol2, users[0]).WithRating(5).Build(),
            ReviewBuilder.Default().For(pol2, users[1]).WithRating(5).Build(),
            ReviewBuilder.Default().For(fra, users[0]).WithRating(5).Build()
        );
        await ctx.SaveChangesAsync();

        _geo.Setup(x => x.ResolveCountryAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("PL");

        var rows = await CreateSut(ctx).GetTopFromCountryAsync(0, 0, 10, default);

        rows.Should().HaveCount(2);
        rows[0].Name.Should().Be(pol2.Name);
        rows[1].Name.Should().Be(pol1.Name);
    }

    [Fact]
    public async Task PerfumeOfTheDay_uses_day_window_first()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var users = UserBuilder.Many(2).ToList();
        var p = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();

        ctx.AddRange(brand);
        ctx.AddRange(users);
        ctx.Add(p);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p, users[0]).WithRating(5).WithDate(DateTime.UtcNow.AddHours(-2)).Build(),
            ReviewBuilder.Default().For(p, users[1]).WithRating(4).WithDate(DateTime.UtcNow.AddHours(-3)).Build()
        );
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto!.Type.Should().Be(PerfumeHighlightTypeEnum.Day);
    }

    [Fact]
    public async Task PerfumeOfTheDay_falls_back_to_recent()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var users = UserBuilder.Many(2).ToList();
        var p = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();

        ctx.AddRange(brand);
        ctx.AddRange(users);
        ctx.Add(p);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(p, users[0]).WithDate(DateTime.UtcNow.AddHours(-30)).Build(),
            ReviewBuilder.Default().For(p, users[1]).WithDate(DateTime.UtcNow.AddHours(-40)).Build()
        );
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto!.Type.Should().Be(PerfumeHighlightTypeEnum.Recent);
    }

    [Fact]
    public async Task PerfumeOfTheDay_falls_back_to_week()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var users = UserBuilder.Many(2).ToList();

        var p =
            PerfumeBuilder.Default()
                .WithId(1)
                .WithBrand(brand)
                .WithPhoto()
                .Build();

        ctx.Add(brand);
        ctx.AddRange(users);
        ctx.Add(p);

        if (p.PerfumePhoto != null)
            ctx.Add(p.PerfumePhoto);

        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default()
                .WithId(1)
                .For(p, users[0])
                .WithDate(DateTime.UtcNow.AddDays(-5))
                .Build(),

            ReviewBuilder.Default()
                .WithId(2)
                .For(p, users[1])
                .WithDate(DateTime.UtcNow.AddDays(-6))
                .Build()
        );

        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto!.Type.Should().Be(PerfumeHighlightTypeEnum.Week);
    }


    [Fact]
    public async Task PerfumeOfTheDay_falls_back_to_community_favorite()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var users = UserBuilder.Many(6).ToList();
        var p = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();

        ctx.AddRange(brand);
        ctx.AddRange(users);
        ctx.Add(p);
        await ctx.SaveChangesAsync();

        foreach (var u in users)
            ctx.Add(ReviewBuilder.Default().For(p, u).WithRating(5).WithDate(DateTime.UtcNow.AddDays(-30)).Build());

        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto!.Type.Should().Be(PerfumeHighlightTypeEnum.Favorite);
    }

    [Fact]
    public async Task PerfumeOfTheDay_falls_back_to_spotlight()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithPhoto().Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithPhoto().Build();

        ctx.AddRange(brand, p1, p2);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto!.Type.Should().Be(PerfumeHighlightTypeEnum.Spotlight);
        dto.PerfumeId.Should().Be(p2.PerfumeId);
    }

    [Fact]
    public async Task PerfumeOfTheDay_returns_null_when_no_spotlight_possible()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var p = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();

        ctx.AddRange(brand, p);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetPerfumeOfTheDayAsync();

        dto.Should().BeNull();
    }
}
