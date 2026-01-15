using Core.DTOs;
using Core.Entities;
using Core.Exceptions;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class ReviewServiceTests
{
    private static ReviewService CreateSut(FragranceLogContext ctx)
        => new(ctx);

    private static async Task<(FragranceLogContext ctx, SqliteConnection conn, Perfume perfume, User user)>
    CreatePerfumeAndUserAsync()
    {
        var (ctx, conn) = DbContextFactory.Create();

        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();
        var user = UserBuilder.Default().Build();

        ctx.AddRange(brand, perfume, user);
        await ctx.SaveChangesAsync();

        return (ctx, conn, perfume, user);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_creates_review_when_not_exists()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).CreateOrUpdateAsync(
            user.UserId,
            new SaveReviewDto
            {
                PerfumeId = perfume.PerfumeId,
                Rating = 5,
                Text = "Great"
            });

        ctx.Reviews.Should().ContainSingle(r =>
            r.UserId == user.UserId &&
            r.PerfumeId == perfume.PerfumeId &&
            r.Rating == 5 &&
            r.Comment == "Great");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_updates_existing_review()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        ctx.Add(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(3)
                .WithComment("Ok")
                .Build()
        );
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).CreateOrUpdateAsync(
            user.UserId,
            new SaveReviewDto
            {
                PerfumeId = perfume.PerfumeId,
                Rating = 5,
                Text = "Excellent"
            });

        ctx.Reviews.Should().ContainSingle(r =>
            r.Rating == 5 &&
            r.Comment == "Excellent");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_normalizes_empty_comment_to_null()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx).CreateOrUpdateAsync(
            user.UserId,
            new SaveReviewDto
            {
                PerfumeId = perfume.PerfumeId,
                Rating = 4,
                Text = "   "
            });

        ctx.Reviews.Should().ContainSingle(r => r.Comment == null);
    }

    [Fact]
    public async Task GetByUserAndPerfumeAsync_returns_null_when_missing()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        var result = await CreateSut(ctx)
            .GetByUserAndPerfumeAsync(user.UserId, perfume.PerfumeId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAndPerfumeAsync_returns_review_dto_when_exists()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        ctx.Add(
            ReviewBuilder.Default()
                .For(perfume, user)
                .WithRating(5)
                .WithComment("Nice")
                .Build()
        );
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx)
            .GetByUserAndPerfumeAsync(user.UserId, perfume.PerfumeId);

        dto.Should().NotBeNull();
        dto!.Rating.Should().Be(5);
        dto.Text.Should().Be("Nice");
        dto.Author.Should().Be(user.Username);
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_review()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        ctx.Add(
            ReviewBuilder.Default()
                .For(perfume, user)
                .Build()
        );
        await ctx.SaveChangesAsync();

        await CreateSut(ctx)
            .DeleteAsync(perfume.PerfumeId, user.UserId, default);

        ctx.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_throws_when_review_not_found()
    {
        var (ctx, conn, perfume, user) = await CreatePerfumeAndUserAsync();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx)
            .Invoking(x => x.DeleteAsync(perfume.PerfumeId, user.UserId, default))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
