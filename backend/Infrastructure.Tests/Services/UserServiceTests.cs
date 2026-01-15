using Core.DTOs;
using Core.Exceptions;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common.Builders;
using Tests.Common;
using Microsoft.Data.Sqlite;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public sealed class UserServiceTests
{
    private static UserService CreateSut(
        FragranceLogContext ctx,
        Mock<IPasswordHasher> hasher)
        => new(ctx, hasher.Object);

    private static Mock<IPasswordHasher> CreateHasherMock(
        bool verifyResult = true,
        string hashed = "hashed-new-password")
    {
        var mock = new Mock<IPasswordHasher>();

        mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(verifyResult);

        mock.Setup(x => x.Hash(It.IsAny<string>()))
            .Returns(hashed);

        return mock;
    }

    [Fact]
    public async Task GetMeAsync_returns_profile()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateHasherMock())
            .GetMeAsync(user.UserId);

        dto.Id.Should().Be(user.UserId);
        dto.DisplayName.Should().Be(user.Username);
        dto.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetMeAsync_throws_when_user_not_found()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx, CreateHasherMock())
            .Invoking(x => x.GetMeAsync(999))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_updates_username_and_email()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateHasherMock())
            .UpdateProfileAsync(
                user.UserId,
                new UpdateProfileDto
                {
                    DisplayName = "new-name",
                    Email = "new@email.test"
                });

        dto.DisplayName.Should().Be("new-name");
        dto.Email.Should().Be("new@email.test");
    }

    [Fact]
    public async Task UpdateProfileAsync_throws_when_email_in_use()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();

        ctx.AddRange(u1, u2);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx, CreateHasherMock())
            .Invoking(x => x.UpdateProfileAsync(
                u1.UserId,
                new UpdateProfileDto
                {
                    DisplayName = u1.Username,
                    Email = u2.Email
                }))
            .Should()
            .ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_throws_when_username_in_use()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var u1 = UserBuilder.Default().WithId(1).Build();
        var u2 = UserBuilder.Default().WithId(2).Build();

        ctx.AddRange(u1, u2);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx, CreateHasherMock())
            .Invoking(x => x.UpdateProfileAsync(
                u1.UserId,
                new UpdateProfileDto
                {
                    DisplayName = u2.Username,
                    Email = u1.Email
                }))
            .Should()
            .ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_allows_same_values()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var dto = await CreateSut(ctx, CreateHasherMock())
            .UpdateProfileAsync(
                user.UserId,
                new UpdateProfileDto
                {
                    DisplayName = user.Username,
                    Email = user.Email
                });

        dto.DisplayName.Should().Be(user.Username);
        dto.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ChangePasswordAsync_updates_password()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var hasher = CreateHasherMock(true, "new-hash");

        await CreateSut(ctx, hasher)
            .ChangePasswordAsync(
                user.UserId,
                new ChangePasswordDto
                {
                    CurrentPassword = "old",
                    NewPassword = "new"
                });

        user.Password.Should().Be("new-hash");
    }

    [Fact]
    public async Task ChangePasswordAsync_throws_when_password_invalid()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        ctx.Add(user);
        await ctx.SaveChangesAsync();

        var hasher = CreateHasherMock(false);

        await CreateSut(ctx, hasher)
            .Invoking(x => x.ChangePasswordAsync(
                user.UserId,
                new ChangePasswordDto
                {
                    CurrentPassword = "bad",
                    NewPassword = "new"
                }))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ChangePasswordAsync_throws_when_user_not_found()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx, CreateHasherMock())
            .Invoking(x => x.ChangePasswordAsync(
                999,
                new ChangePasswordDto
                {
                    CurrentPassword = "x",
                    NewPassword = "y"
                }))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAccountAsync_soft_deletes_user_and_removes_lists()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        var user = UserBuilder.Default().Build();
        var list = PerfumeListBuilder.Default().ForUser(user).Build();

        ctx.AddRange(user, list);
        await ctx.SaveChangesAsync();

        await CreateSut(ctx, CreateHasherMock())
            .DeleteAccountAsync(user.UserId);

        ctx.PerfumeLists.Should().BeEmpty();

        user.Username.Should().Be($"deleted_user_{user.UserId}");
        user.Email.Should().Be($"deleted+{user.UserId}@fragrance.log");
        user.Password.Length.Should().Be(64);
    }

    [Fact]
    public async Task DeleteAccountAsync_does_nothing_when_user_not_found()
    {
        var (ctx, conn) = DbContextFactory.Create();
        using var _ = conn;
        using var __ = ctx;

        await CreateSut(ctx, CreateHasherMock())
            .DeleteAccountAsync(999);

        ctx.Users.Should().BeEmpty();
    }
}
