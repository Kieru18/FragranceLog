using Core.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common.Builders;
using Tests.Common;
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

    private static async Task<(FragranceLogContext ctx, SqliteConnection conn, Perfume perfume)>
        CreatePerfumeWithVotes(Action<FragranceLogContext, Perfume> configure)
    {
        var (ctx, conn) = DbContextFactory.Create();

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(brand, perfume);
        await ctx.SaveChangesAsync();

        configure(ctx, perfume);
        await ctx.SaveChangesAsync();

        return (ctx, conn, perfume);
    }

    [Fact]
    public async Task SearchAsync_filters_by_gender_majority_vote()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();
        var u3 = UserBuilder.Default().WithId(3).Build();

        var male = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var female = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(brand, u1, u2, u3, male, female);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            PerfumeGenderVoteBuilder.For(male, u1, GenderEnum.Male),
            PerfumeGenderVoteBuilder.For(male, u2, GenderEnum.Male),
            PerfumeGenderVoteBuilder.For(male, u3, GenderEnum.Female),

            PerfumeGenderVoteBuilder.For(female, u1, GenderEnum.Female),
            PerfumeGenderVoteBuilder.For(female, u2, GenderEnum.Female),
            PerfumeGenderVoteBuilder.For(female, u3, GenderEnum.Male)
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new() { Gender = GenderEnum.Male },
            default);

        result.Items.Should().ContainSingle();
        result.Items[0].PerfumeId.Should().Be(male.PerfumeId);
    }

    [Fact]
    public async Task SearchAsync_filters_by_all_groups_not_any()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var g1 = GroupBuilder.Default().WithId(1).Build();
        var g2 = GroupBuilder.Default().WithId(2).Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithGroups(g1, g2).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithGroups(g1).Build();

        ctx.AddRange(brand, g1, g2, p1, p2);
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new() { GroupIds = new[] { 1, 2 } },
            default);

        result.Items.Should().ContainSingle();
        result.Items[0].PerfumeId.Should().Be(p1.PerfumeId);
    }

    [Fact]
    public async Task SearchAsync_excludes_unrated_perfumes_when_min_rating_applied()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var user = UserBuilder.Default().Build();

        var rated = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var unrated = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(brand, user, rated, unrated);
        await ctx.SaveChangesAsync();

        ctx.Add(ReviewBuilder.Default().For(rated, user).WithRating(5).Build());
        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new() { MinRating = 1 },
            default);

        result.Items.Should().ContainSingle();
        result.Items[0].PerfumeId.Should().Be(rated.PerfumeId);
    }

    [Fact]
    public async Task SearchAsync_includes_unrated_perfumes_when_no_min_rating()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();

        ctx.AddRange(
            brand,
            PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build(),
            PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build()
        );

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(new(), default);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_normalizes_page_and_page_size()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();

        var perfumes = Enumerable.Range(1, 30)
            .Select(i => PerfumeBuilder.Default().WithId(i).WithBrand(brand).Build())
            .ToList();

        ctx.Add(brand);
        ctx.AddRange(perfumes);

        await ctx.SaveChangesAsync();

        var result = await CreateSut(ctx).SearchAsync(
            new PerfumeSearchRequestDto { Page = 0, PageSize = 100 },
            default);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.Items.Should().HaveCount(25);
    }

    [Fact]
    public async Task GetDetailsAsync_aggregates_votes_correctly()
    {
        var (ctx, conn, perfume) = await CreatePerfumeWithVotes((ctx, p) =>
        {
            var u1 = UserBuilder.Default().WithId(1).Build();
            var u2 = UserBuilder.Default().WithId(2).Build();
            var u3 = UserBuilder.Default().WithId(3).Build();

            ctx.AddRange(u1, u2, u3);

            ctx.AddRange(
                PerfumeGenderVoteBuilder.For(p, u1, GenderEnum.Unisex),
                PerfumeGenderVoteBuilder.For(p, u2, GenderEnum.Unisex),
                PerfumeGenderVoteBuilder.For(p, u3, GenderEnum.Male),

                PerfumeLongevityVoteBuilder.For(p, u1, LongevityEnum.Moderate),
                PerfumeLongevityVoteBuilder.For(p, u2, LongevityEnum.Moderate),
                PerfumeLongevityVoteBuilder.For(p, u3, LongevityEnum.LongLasting)
            );
        });

        using var _ = conn;
        using var __ = ctx;

        var dto = await CreateSut(ctx).GetDetailsAsync(perfume.PerfumeId, null, default);

        dto.Gender.Should().Be(GenderEnum.Unisex);
        dto.Longevity.Should().BeApproximately(3.33, 0.01);
    }

    [Fact]
    public async Task GetDetailsAsync_returns_null_user_specific_fields_when_user_is_null()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(brand, perfume);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetDetailsAsync(perfume.PerfumeId, null, default);

        dto.MyRating.Should().BeNull();
        dto.MyReview.Should().BeNull();
        dto.MyGenderVote.Should().BeNull();
        dto.MySeasonVote.Should().BeNull();
        dto.MyDaytimeVote.Should().BeNull();
        dto.MyLongevityVote.Should().BeNull();
        dto.MySillageVote.Should().BeNull();
    }

    [Fact]
    public async Task GetDetailsAsync_limits_reviews_to_20_and_orders_by_date_desc()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();
        var user = UserBuilder.Default().Build();

        ctx.AddRange(brand, perfume, user);
        await ctx.SaveChangesAsync();

        for (var i = 1; i <= 30; i++)
        {
            ctx.Add(
                ReviewBuilder.Default()
                    .For(perfume, user)
                    .WithRating(5)
                    .WithComment($"R{i}")
                    .WithDate(DateTime.UtcNow.AddMinutes(i))
                    .Build()
            );
        }

        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetDetailsAsync(perfume.PerfumeId, null, default);

        dto.Reviews.Should().HaveCount(20);
        dto.Reviews.First().Text.Should().Be("R30");
    }
}
