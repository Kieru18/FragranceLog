using Core.Enums;
using Core.Exceptions;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class PerfumeServiceTests
{
    private static PerfumeService CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task SearchAsync_applies_query_relevance_and_rating_ordering()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithName("Alpha").Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithName("Alpha Plus").Build();
        var p3 = PerfumeBuilder.Default().WithId(3).WithBrand(brand).WithName("Something Alpha").Build();

        ctx.AddRange(brand, p1, p2, p3);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new()
            {
                Query = "alpha",
                Page = 1,
                PageSize = 10
            },
            default);

        result.Items.Select(x => x.Name)
            .Should()
            .Equal("Alpha", "Alpha Plus", "Something Alpha");
    }

    [Fact]
    public async Task SearchAsync_filters_by_brand_country_and_min_rating()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand1 = BrandBuilder.Default().WithId(1).Build();
        var brand2 = BrandBuilder.Default().WithId(2).Build();
        var user = UserBuilder.Default().Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand1).WithCountry("POL").Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand2).WithCountry("FRA").Build();

        ctx.AddRange(brand1, brand2, user, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.Add(ReviewBuilder.Default().For(p1, user).WithRating(5).Build());
        ctx.Add(ReviewBuilder.Default().For(p2, user).WithRating(3).Build());
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new()
            {
                BrandId = brand1.BrandId,
                CountryCode = "POL",
                MinRating = 4
            },
            default);

        result.Items.Should().ContainSingle();
        result.Items[0].PerfumeId.Should().Be(p1.PerfumeId);
    }

    [Fact]
    public async Task SearchAsync_applies_pagination_and_total_count()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();

        for (var i = 1; i <= 30; i++)
            ctx.Add(PerfumeBuilder.Default().WithId(i).WithBrand(brand).WithName($"P{i}").Build());

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new()
            {
                Page = 2,
                PageSize = 10
            },
            default);

        result.TotalCount.Should().Be(30);
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetDetailsAsync_returns_full_details_and_user_specific_votes()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var brand = BrandBuilder.Default().Build();

        var perfume = PerfumeBuilder.Default()
            .WithId(1)
            .WithBrand(brand)
            .WithPhoto()
            .Build();

        ctx.AddRange(user, other, brand, perfume);

        if (perfume.PerfumePhoto != null)
            ctx.Add(perfume.PerfumePhoto);

        await ctx.SaveChangesAsync();

        ctx.AddRange(
            ReviewBuilder.Default().For(perfume, user).WithRating(5).WithComment("Great").Build(),
            ReviewBuilder.Default().For(perfume, other).WithRating(4).WithComment("Nice").Build()
        );

        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetDetailsAsync(perfume.PerfumeId, user.UserId, default);

        dto.Name.Should().Be(perfume.Name);
        dto.Brand.Should().Be(brand.Name);
        dto.AvgRating.Should().Be(4.5);
        dto.RatingCount.Should().Be(2);
        dto.CommentCount.Should().Be(2);
        dto.MyRating.Should().Be(5);
        dto.MyReview.Should().Be("Great");
    }

    [Fact]
    public async Task GetDetailsAsync_throws_when_perfume_not_found()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx)
            .Invoking(x => x.GetDetailsAsync(999, null, default))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
