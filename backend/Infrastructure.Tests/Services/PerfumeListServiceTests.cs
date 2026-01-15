using Core.DTOs;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class PerfumeListServiceTests
{
    private static PerfumeListService CreateSut(FragranceLogContext ctx)
        => new(ctx);

    [Fact]
    public async Task GetUserListsAsync_returns_only_users_lists_ordered_by_system_then_name()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();

        var sysB = PerfumeListBuilder.Default().WithId(1).ForUser(user).AsSystem().WithName("Wishlist").Build();
        var sysA = PerfumeListBuilder.Default().WithId(2).ForUser(user).AsSystem().WithName("Owned").Build();
        var cB = PerfumeListBuilder.Default().WithId(3).ForUser(user).WithName("Custom B").Build();
        var cA = PerfumeListBuilder.Default().WithId(4).ForUser(user).WithName("Custom A").Build();
        var otherList = PerfumeListBuilder.Default().WithId(5).ForUser(other).WithName("Other").Build();

        ctx.AddRange(user, other);
        ctx.AddRange(sysB, sysA, cB, cA, otherList);
        await ctx.SaveChangesAsync();

        var rows = await CreateSut(ctx).GetUserListsAsync(user.UserId);

        rows.Should().HaveCount(4);
        rows.Select(x => x.Name).Should().Equal("Owned", "Wishlist", "Custom A", "Custom B");
        rows.All(x => x.IsSystem).Should().BeFalse();
        rows.Count(x => x.IsSystem).Should().Be(2);
    }

    [Fact]
    public async Task GetListsOverviewAsync_returns_counts_and_preview_images_limited_to_6_and_ordered()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var brand = BrandBuilder.Default().Build();

        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("My List").Build();
        var empty = PerfumeListBuilder.Default().WithId(2).ForUser(user).WithName("Empty").Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p3 = PerfumeBuilder.Default().WithId(3).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p4 = PerfumeBuilder.Default().WithId(4).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p5 = PerfumeBuilder.Default().WithId(5).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p6 = PerfumeBuilder.Default().WithId(6).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p7 = PerfumeBuilder.Default().WithId(7).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var p8 = PerfumeBuilder.Default().WithId(8).WithBrand(brand).WithCountry("POL").WithPhoto().Build();

        ctx.AddRange(user, brand);
        ctx.AddRange(list, empty);
        ctx.AddRange(p1, p2, p3, p4, p5, p6, p7, p8);

        if (p1.PerfumePhoto != null) ctx.Add(p1.PerfumePhoto);
        if (p2.PerfumePhoto != null) ctx.Add(p2.PerfumePhoto);
        if (p3.PerfumePhoto != null) ctx.Add(p3.PerfumePhoto);
        if (p4.PerfumePhoto != null) ctx.Add(p4.PerfumePhoto);
        if (p5.PerfumePhoto != null) ctx.Add(p5.PerfumePhoto);
        if (p6.PerfumePhoto != null) ctx.Add(p6.PerfumePhoto);
        if (p7.PerfumePhoto != null) ctx.Add(p7.PerfumePhoto);
        if (p8.PerfumePhoto != null) ctx.Add(p8.PerfumePhoto);

        ctx.AddRange(
            PerfumeListItemBuilder.Default().WithId(1).For(list, p8).Build(),
            PerfumeListItemBuilder.Default().WithId(2).For(list, p7).Build(),
            PerfumeListItemBuilder.Default().WithId(3).For(list, p6).Build(),
            PerfumeListItemBuilder.Default().WithId(4).For(list, p5).Build(),
            PerfumeListItemBuilder.Default().WithId(5).For(list, p4).Build(),
            PerfumeListItemBuilder.Default().WithId(6).For(list, p3).Build(),
            PerfumeListItemBuilder.Default().WithId(7).For(list, p2).Build(),
            PerfumeListItemBuilder.Default().WithId(8).For(list, p1).Build()
        );

        await ctx.SaveChangesAsync();

        var rows = await CreateSut(ctx).GetListsOverviewAsync(user.UserId);

        rows.Should().HaveCount(2);
        rows.Select(x => x.Name).Should().Equal("Empty", "My List");

        var myList = rows.Single(x => x.Name == "My List");
        myList.PerfumeCount.Should().Be(8);

        myList.PreviewImages.Should().HaveCount(6);
        myList.PreviewImages.All(x => !string.IsNullOrWhiteSpace(x)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateListAsync_creates_non_system_list_and_blocks_duplicate_name_for_same_user()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();

        ctx.AddRange(user, other);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        var dto = await sut.CreateListAsync(user.UserId, "Favorites");

        dto.Name.Should().Be("Favorites");
        dto.IsSystem.Should().BeFalse();
        dto.PerfumeListId.Should().BeGreaterThan(0);

        await sut.Invoking(x => x.CreateListAsync(user.UserId, "Favorites"))
            .Should()
            .ThrowAsync<InvalidOperationException>();

        await sut.Invoking(x => x.CreateListAsync(other.UserId, "Favorites"))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task RenameListAsync_renames_owned_non_system_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("Old").Build();

        ctx.Add(user);
        ctx.Add(list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).RenameListAsync(user.UserId, list.PerfumeListId, "New");

        var stored = await ctx.PerfumeLists.FindAsync(list.PerfumeListId);
        stored!.Name.Should().Be("New");
    }

    [Fact]
    public async Task RenameListAsync_throws_for_system_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).AsSystem().WithName("Owned").Build();

        ctx.Add(user);
        ctx.Add(list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.RenameListAsync(user.UserId, list.PerfumeListId, "Nope"))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RenameListAsync_throws_when_not_owner()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("X").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.RenameListAsync(other.UserId, list.PerfumeListId, "Y"))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteListAsync_deletes_owned_non_system_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("Temp").Build();

        ctx.AddRange(user, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).DeleteListAsync(user.UserId, list.PerfumeListId);

        var exists = await ctx.PerfumeLists.AnyAsync(x => x.PerfumeListId == list.PerfumeListId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteListAsync_throws_for_system_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).AsSystem().WithName("Owned").Build();

        ctx.AddRange(user, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.DeleteListAsync(user.UserId, list.PerfumeListId))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteListAsync_throws_when_not_owner()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("X").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.DeleteListAsync(other.UserId, list.PerfumeListId))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetListPerfumesAsync_returns_perfumes_with_aggregates_and_my_rating_and_photo_nullable()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var brand = BrandBuilder.Default().Build();

        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("L").Build();

        var withPhoto = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").WithPhoto().Build();
        var noPhoto = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithCountry("POL").Build();

        ctx.AddRange(user, other, brand, list, withPhoto, noPhoto);

        if (withPhoto.PerfumePhoto != null)
            ctx.Add(withPhoto.PerfumePhoto);

        ctx.AddRange(
            PerfumeListItemBuilder.Default().WithId(1).For(list, withPhoto).Build(),
            PerfumeListItemBuilder.Default().WithId(2).For(list, noPhoto).Build()
        );

        ctx.AddRange(
            ReviewBuilder.Default().WithId(1).For(withPhoto, user).WithRating(4).Build(),
            ReviewBuilder.Default().WithId(2).For(withPhoto, other).WithRating(2).Build(),
            ReviewBuilder.Default().WithId(3).For(noPhoto, other).WithRating(5).Build()
        );

        await ctx.SaveChangesAsync();

        var rows = await CreateSut(ctx).GetListPerfumesAsync(user.UserId, list.PerfumeListId);

        rows.Should().HaveCount(2);

        var row1 = rows.Single(x => x.PerfumeId == withPhoto.PerfumeId);
        row1.Brand.Should().Be(withPhoto.Brand.Name);
        row1.ImageUrl.Should().NotBeNull();
        row1.AvgRating.Should().Be(3);
        row1.RatingCount.Should().Be(2);
        row1.MyRating.Should().Be(4);

        var row2 = rows.Single(x => x.PerfumeId == noPhoto.PerfumeId);
        row2.ImageUrl.Should().BeNull();
        row2.AvgRating.Should().Be(5);
        row2.RatingCount.Should().Be(1);
        row2.MyRating.Should().BeNull();
    }

    [Fact]
    public async Task GetListPerfumesAsync_throws_when_not_owner()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();

        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("L").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.GetListPerfumesAsync(other.UserId, list.PerfumeListId))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task AddPerfumeToListAsync_adds_once_and_is_idempotent()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var brand = BrandBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("L").Build();
        var perfume = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").Build();

        ctx.AddRange(user, brand, list, perfume);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        await sut.AddPerfumeToListAsync(user.UserId, list.PerfumeListId, perfume.PerfumeId);
        await sut.AddPerfumeToListAsync(user.UserId, list.PerfumeListId, perfume.PerfumeId);

        var count = ctx.PerfumeListItems.Count(x => x.PerfumeListId == list.PerfumeListId && x.PerfumeId == perfume.PerfumeId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddPerfumeToListAsync_throws_when_not_owner()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("L").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.AddPerfumeToListAsync(other.UserId, list.PerfumeListId, 123))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RemovePerfumeFromListAsync_removes_existing_and_noops_when_missing()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var brand = BrandBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(user).WithName("L").Build();
        var perfume = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").Build();

        var item = PerfumeListItemBuilder.Default().WithId(1).For(list, perfume).Build();

        ctx.AddRange(user, brand, list, perfume, item);
        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx);

        await sut.RemovePerfumeFromListAsync(user.UserId, list.PerfumeListId, perfume.PerfumeId);

        ctx.PerfumeListItems.Any(x => x.PerfumeListId == list.PerfumeListId && x.PerfumeId == perfume.PerfumeId)
            .Should()
            .BeFalse();

        await sut.RemovePerfumeFromListAsync(user.UserId, list.PerfumeListId, perfume.PerfumeId);

        ctx.PerfumeListItems.Should().BeEmpty();
    }

    [Fact]
    public async Task RemovePerfumeFromListAsync_throws_when_not_owner()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();
        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("L").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx).Invoking(x => x.RemovePerfumeFromListAsync(other.UserId, list.PerfumeListId, 1))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetListsForPerfumeAsync_returns_all_lists_with_contains_flag_and_orders()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var brand = BrandBuilder.Default().Build();
        var perfume = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithCountry("POL").Build();

        var sys = PerfumeListBuilder.Default().WithId(1).ForUser(user).AsSystem().WithName("Owned").Build();
        var inList = PerfumeListBuilder.Default().WithId(2).ForUser(user).WithName("A").Build();
        var outList = PerfumeListBuilder.Default().WithId(3).ForUser(user).WithName("B").Build();

        ctx.AddRange(user, brand, perfume, sys, inList, outList);
        ctx.Add(PerfumeListItemBuilder.Default().WithId(1).For(inList, perfume).Build());
        await ctx.SaveChangesAsync();

        var rows = await CreateSut(ctx).GetListsForPerfumeAsync(user.UserId, perfume.PerfumeId);

        rows.Select(x => x.Name).Should().Equal("Owned", "A", "B");
        rows.Single(x => x.Name == "A").ContainsPerfume.Should().BeTrue();
        rows.Single(x => x.Name == "B").ContainsPerfume.Should().BeFalse();
        rows.Single(x => x.Name == "Owned").ContainsPerfume.Should().BeFalse();
    }

    [Fact]
    public async Task GetListAsync_returns_dto_for_owned_list_and_throws_when_not_owned()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var other = UserBuilder.Default().WithId(2).Build();

        var list = PerfumeListBuilder.Default().WithId(1).ForUser(owner).WithName("L").Build();

        ctx.AddRange(owner, other, list);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx).GetListAsync(owner.UserId, list.PerfumeListId);

        dto.PerfumeListId.Should().Be(list.PerfumeListId);
        dto.Name.Should().Be("L");

        await CreateSut(ctx).Invoking(x => x.GetListAsync(other.UserId, list.PerfumeListId))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }
}
