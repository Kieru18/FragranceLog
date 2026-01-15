using Core.Dtos;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Tests.Builders;
using Infrastructure.Tests.Common;
using Microsoft.Data.Sqlite;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class SharedListServiceTests
{
    private static SharedListService CreateSut(
        FragranceLogContext ctx,
        Mock<IPerfumeListService> listService)
        => new(ctx, listService.Object);

    private static Mock<IPerfumeListService> CreateListServiceMock()
    {
        var mock = new Mock<IPerfumeListService>();

        mock.Setup(x => x.CreateListAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((int userId, string name) =>
                new PerfumeListDto
                {
                    PerfumeListId = 100,
                    Name = name
                });

        mock.Setup(x =>
                x.AddPerfumeToListAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    [Fact]
    public async Task ShareAsync_throws_when_user_does_not_own_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().WithId(1).Build();
        var owner = UserBuilder.Default().WithId(999).Build();

        var list = PerfumeListBuilder.Default()
            .ForUser(owner)
            .Build();

        ctx.AddRange(user, owner, list);

        await ctx.SaveChangesAsync();

        var sut = CreateSut(ctx, CreateListServiceMock());

        await sut
            .Invoking(x => x.ShareAsync(user.UserId, list.PerfumeListId))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ShareAsync_creates_shared_list_when_not_exists()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();

        ctx.AddRange(user, list);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateListServiceMock())
            .ShareAsync(user.UserId, list.PerfumeListId);

        dto.ShareToken.Should().NotBeEmpty();
        ctx.SharedLists.Should().ContainSingle();
    }

    [Fact]
    public async Task ShareAsync_returns_existing_active_shared_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();
        var shared = new SharedList
        {
            PerfumeList = list,
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = user.UserId,
            ShareToken = Guid.NewGuid()
        };

        ctx.AddRange(user, list, shared);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateListServiceMock())
            .ShareAsync(user.UserId, list.PerfumeListId);

        dto.ShareToken.Should().Be(shared.ShareToken);
        ctx.SharedLists.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPreviewAsync_throws_when_token_invalid()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var sut = CreateSut(ctx, CreateListServiceMock());

        await sut
            .Invoking(x => x.GetPreviewAsync(Guid.NewGuid()))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetPreviewAsync_returns_preview_with_perfumes()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(owner).Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).Build();

        ctx.AddRange(owner, brand, list, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            new PerfumeListItem { PerfumeListId = list.PerfumeListId, PerfumeId = p1.PerfumeId },
            new PerfumeListItem { PerfumeListId = list.PerfumeListId, PerfumeId = p2.PerfumeId }
        );

        ctx.Add(
            new SharedList
            {
                PerfumeListId = list.PerfumeListId,
                OwnerUserId = owner.UserId,
                ShareToken = Guid.NewGuid()
            });

        await ctx.SaveChangesAsync();

        var preview = await CreateSut(ctx, CreateListServiceMock())
            .GetPreviewAsync(ctx.SharedLists.Single().ShareToken);

        preview.ListName.Should().Be(list.Name);
        preview.OwnerName.Should().Be(owner.Username);
        preview.Perfumes.Should().HaveCount(2);
    }

    [Fact]
    public async Task ImportAsync_creates_list_and_imports_items()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var target = UserBuilder.Default().WithId(2).Build();
        var brand = BrandBuilder.Default().Build();

        var list = PerfumeListBuilder.Default().ForUser(owner).WithName("My List").Build();
        var perfume = PerfumeBuilder.Default().WithBrand(brand).Build();

        ctx.AddRange(owner, target, brand, list, perfume);
        await ctx.SaveChangesAsync();

        ctx.Add(new PerfumeListItem
        {
            PerfumeListId = list.PerfumeListId,
            PerfumeId = perfume.PerfumeId
        });

        var shared = new SharedList
        {
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = owner.UserId,
            ShareToken = Guid.NewGuid()
        };

        ctx.Add(shared);
        await ctx.SaveChangesAsync();

        var listService = CreateListServiceMock();

        var newListId = await CreateSut(ctx, listService)
            .ImportAsync(target.UserId, shared.ShareToken);

        newListId.Should().Be(100);

        listService.Verify(
            x => x.AddPerfumeToListAsync(
                target.UserId,
                100,
                perfume.PerfumeId),
            Times.Once);
    }

    [Fact]
    public async Task ShareAsync_replaces_expired_shared_list()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();

        var expired = new SharedList
        {
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = user.UserId,
            ShareToken = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        ctx.AddRange(user, list, expired);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateListServiceMock())
            .ShareAsync(user.UserId, list.PerfumeListId);

        dto.ShareToken.Should().NotBe(expired.ShareToken);

        ctx.SharedLists.Should().ContainSingle();
        ctx.SharedLists.Single().ExpirationDate.Should().BeNull();
    }

    [Fact]
    public async Task GetPreviewAsync_throws_when_shared_list_expired()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();

        var expired = new SharedList
        {
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = user.UserId,
            ShareToken = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddMinutes(-5)
        };

        ctx.AddRange(user, list, expired);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx, CreateListServiceMock())
            .Invoking(x => x.GetPreviewAsync(expired.ShareToken))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetPreviewAsync_orders_by_avg_rating_desc_then_name()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().Build();
        var brand = BrandBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(owner).Build();

        var p1 = PerfumeBuilder.Default().WithId(1).WithBrand(brand).WithName("B").Build();
        var p2 = PerfumeBuilder.Default().WithId(2).WithBrand(brand).WithName("A").Build();

        ctx.AddRange(owner, brand, list, p1, p2);
        await ctx.SaveChangesAsync();

        ctx.AddRange(
            new PerfumeListItem { PerfumeListId = list.PerfumeListId, PerfumeId = p1.PerfumeId },
            new PerfumeListItem { PerfumeListId = list.PerfumeListId, PerfumeId = p2.PerfumeId },

            ReviewBuilder.Default().For(p1, owner).WithRating(3).Build(),
            ReviewBuilder.Default().For(p2, owner).WithRating(5).Build()
        );

        ctx.Add(new SharedList
        {
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = owner.UserId,
            ShareToken = Guid.NewGuid()
        });

        await ctx.SaveChangesAsync();

        var preview = await CreateSut(ctx, CreateListServiceMock())
            .GetPreviewAsync(ctx.SharedLists.Single().ShareToken);

        preview.Perfumes.Select(x => x.Name)
            .Should()
            .Equal("A", "B");
    }

    [Fact]
    public async Task ImportAsync_appends_suffix_when_list_name_exists()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var owner = UserBuilder.Default().WithId(1).Build();
        var target = UserBuilder.Default().WithId(2).Build();

        var existing = PerfumeListBuilder.Default()
            .WithId(1)
            .ForUser(target)
            .WithName("My List")
            .Build();

        var sharedList = PerfumeListBuilder.Default()
            .WithId(2)
            .ForUser(owner)
            .WithName("My List")
            .Build();

        ctx.AddRange(owner, target, existing, sharedList);
        await ctx.SaveChangesAsync();

        var shared = new SharedList
        {
            PerfumeListId = sharedList.PerfumeListId,
            OwnerUserId = owner.UserId,
            ShareToken = Guid.NewGuid()
        };

        ctx.Add(shared);
        await ctx.SaveChangesAsync();

        var mock = CreateListServiceMock();

        await CreateSut(ctx, mock)
            .ImportAsync(target.UserId, shared.ShareToken);

        mock.Verify(
            x => x.CreateListAsync(
                target.UserId,
                "My List (1)"),
            Times.Once);
    }

    [Fact]
    public async Task ImportAsync_throws_when_shared_list_expired()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();

        var expired = new SharedList
        {
            PerfumeListId = list.PerfumeListId,
            OwnerUserId = user.UserId,
            ShareToken = Guid.NewGuid(),
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };

        ctx.AddRange(user, list, expired);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx, CreateListServiceMock())
            .Invoking(x => x.ImportAsync(999, expired.ShareToken))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }
}
