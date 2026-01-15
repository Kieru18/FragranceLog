using Core.DTOs;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Tests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Infrastructure.Tests.Services
{
    public sealed class AuthServiceTests
    {
        private readonly Mock<IPasswordHasher> _hasher;
        private readonly Mock<IJwtService> _jwt;

        public AuthServiceTests()
        {
            _hasher = new Mock<IPasswordHasher>();
            _jwt = new Mock<IJwtService>();
        }

        private AuthService CreateService(FragranceLogContext ctx)
        {
            return new AuthService(ctx, _hasher.Object, _jwt.Object);
        }

        private static ClaimsPrincipal CreatePrincipalWithUserId(string userId)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, userId)
                    },
                    "jwt"));
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUserAndSystemLists()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            _hasher.Setup(h => h.Hash("Password1!")).Returns("HASH");
            _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(It.IsAny<int>())).Returns("REFRESH");

            var service = CreateService(ctx);

            var dto = new RegisterDto
            {
                Username = "user",
                Email = "user@test.com",
                Password = "Password1!"
            };

            var result = await service.RegisterAsync(dto);

            result.AccessToken.Should().Be("ACCESS");
            result.RefreshToken.Should().Be("REFRESH");

            var user = await ctx.Users.SingleAsync();
            user.Username.Should().Be("user");
            user.Email.Should().Be("user@test.com");
            user.Password.Should().Be("HASH");

            var lists = await ctx.PerfumeLists
                .Where(l => l.UserId == user.UserId)
                .ToListAsync();

            lists.Should().HaveCount(2);
            lists.Select(l => l.Name).Should().Contain(new[] { "Owned", "Wanted" });
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenUsernameAlreadyExists()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            ctx.Users.Add(new User
            {
                Username = "user",
                Email = "other@test.com",
                Password = "HASH"
            });
            await ctx.SaveChangesAsync();

            var service = CreateService(ctx);

            var dto = new RegisterDto
            {
                Username = "user",
                Email = "new@test.com",
                Password = "Password1!"
            };

            await FluentActions
                .Invoking(() => service.RegisterAsync(dto))
                .Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            ctx.Users.Add(new User
            {
                Username = "other",
                Email = "user@test.com",
                Password = "HASH"
            });
            await ctx.SaveChangesAsync();

            var service = CreateService(ctx);

            var dto = new RegisterDto
            {
                Username = "user",
                Email = "user@test.com",
                Password = "Password1!"
            };

            await FluentActions
                .Invoking(() => service.RegisterAsync(dto))
                .Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokens_WhenLoginByUsernameIsValid()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("Password1!", "HASH")).Returns(true);
            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("REFRESH");

            var service = CreateService(ctx);

            var dto = new LoginDto
            {
                UsernameOrEmail = "user",
                Password = "Password1!"
            };

            var result = await service.LoginAsync(dto);

            result.AccessToken.Should().Be("ACCESS");
            result.RefreshToken.Should().Be("REFRESH");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokens_WhenLoginByEmailIsValid()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("Password1!", "HASH")).Returns(true);
            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("REFRESH");

            var service = CreateService(ctx);

            var dto = new LoginDto
            {
                UsernameOrEmail = "user@test.com",
                Password = "Password1!"
            };

            var result = await service.LoginAsync(dto);

            result.AccessToken.Should().Be("ACCESS");
            result.RefreshToken.Should().Be("REFRESH");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            var service = CreateService(ctx);

            var dto = new LoginDto
            {
                UsernameOrEmail = "missing",
                Password = "Password1!"
            };

            await FluentActions
                .Invoking(() => service.LoginAsync(dto))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("bad", "HASH")).Returns(false);

            var service = CreateService(ctx);

            var dto = new LoginDto
            {
                UsernameOrEmail = "user",
                Password = "bad"
            };

            await FluentActions
                .Invoking(() => service.LoginAsync(dto))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenTokenIsMissing()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;
            var service = CreateService(ctx);

            await FluentActions
                .Invoking(() => service.RefreshAsync(""))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenTokenIsInvalid()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;
            var service = CreateService(ctx);

            _jwt.Setup(j => j.ValidateRefreshToken("bad"))
                .Returns((ClaimsPrincipal?)null);

            await FluentActions
                .Invoking(() => service.RefreshAsync("bad"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserIdClaimIsMissing()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;
            var service = CreateService(ctx);

            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            _jwt.Setup(j => j.ValidateRefreshToken("token")).Returns(principal);

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserIdClaimIsNotNumeric()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;
            var service = CreateService(ctx);

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId("abc"));

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserDoesNotExist()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;
            var service = CreateService(ctx);

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId("1"));

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldReturnNewTokens_WhenTokenIsValid()
        {
            var (ctx, conn) = DbContextFactory.Create();
            using var _ = conn;
            using var __ = ctx;

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId(user.UserId.ToString()));

            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("NEW_ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("NEW_REFRESH");

            var service = CreateService(ctx);

            var result = await service.RefreshAsync("token");

            result.AccessToken.Should().Be("NEW_ACCESS");
            result.RefreshToken.Should().Be("NEW_REFRESH");
        }
    }
}
