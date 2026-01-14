using Core.DTOs;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Tests.Common;
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

        private AuthService CreateService(FragranceLogContext context)
        {
            return new AuthService(context, _hasher.Object, _jwt.Object);
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
            using var context = DbContextFactory.Create();

            _hasher.Setup(h => h.Hash("Password1!")).Returns("HASH");
            _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(It.IsAny<int>())).Returns("REFRESH");

            var service = CreateService(context);

            var dto = new RegisterDto
            {
                Username = "user",
                Email = "user@test.com",
                Password = "Password1!"
            };

            var result = await service.RegisterAsync(dto);

            result.AccessToken.Should().Be("ACCESS");
            result.RefreshToken.Should().Be("REFRESH");

            var user = await context.Users.SingleAsync();
            user.Username.Should().Be("user");
            user.Email.Should().Be("user@test.com");
            user.Password.Should().Be("HASH");

            var lists = await context.PerfumeLists
                .Where(l => l.UserId == user.UserId)
                .ToListAsync();

            lists.Should().HaveCount(2);
            lists.Select(l => l.Name).Should().Contain(new[] { "Owned", "Wanted" });
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenUsernameAlreadyExists()
        {
            using var context = DbContextFactory.Create();

            context.Users.Add(new User
            {
                Username = "user",
                Email = "other@test.com",
                Password = "HASH"
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();

            context.Users.Add(new User
            {
                Username = "other",
                Email = "user@test.com",
                Password = "HASH"
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("Password1!", "HASH")).Returns(true);
            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("REFRESH");

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("Password1!", "HASH")).Returns(true);
            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("REFRESH");

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            _hasher.Setup(h => h.Verify("bad", "HASH")).Returns(false);

            var service = CreateService(context);

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
            using var context = DbContextFactory.Create();
            var service = CreateService(context);

            await FluentActions
                .Invoking(() => service.RefreshAsync(""))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenTokenIsInvalid()
        {
            using var context = DbContextFactory.Create();
            var service = CreateService(context);

            _jwt.Setup(j => j.ValidateRefreshToken("bad"))
                .Returns((ClaimsPrincipal?)null);

            await FluentActions
                .Invoking(() => service.RefreshAsync("bad"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserIdClaimIsMissing()
        {
            using var context = DbContextFactory.Create();
            var service = CreateService(context);

            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            _jwt.Setup(j => j.ValidateRefreshToken("token")).Returns(principal);

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserIdClaimIsNotNumeric()
        {
            using var context = DbContextFactory.Create();
            var service = CreateService(context);

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId("abc"));

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldThrow_WhenUserDoesNotExist()
        {
            using var context = DbContextFactory.Create();
            var service = CreateService(context);

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId("1"));

            await FluentActions
                .Invoking(() => service.RefreshAsync("token"))
                .Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task RefreshAsync_ShouldReturnNewTokens_WhenTokenIsValid()
        {
            using var context = DbContextFactory.Create();

            var user = new User
            {
                Username = "user",
                Email = "user@test.com",
                Password = "HASH"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            _jwt.Setup(j => j.ValidateRefreshToken("token"))
                .Returns(CreatePrincipalWithUserId(user.UserId.ToString()));

            _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("NEW_ACCESS");
            _jwt.Setup(j => j.GenerateRefreshToken(user.UserId)).Returns("NEW_REFRESH");

            var service = CreateService(context);

            var result = await service.RefreshAsync("token");

            result.AccessToken.Should().Be("NEW_ACCESS");
            result.RefreshToken.Should().Be("NEW_REFRESH");
        }
    }
}
