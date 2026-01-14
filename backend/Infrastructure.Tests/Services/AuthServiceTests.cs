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
            user.Password.Should().Be("HASH");

            var lists = await context.PerfumeLists
                .Where(l => l.UserId == user.UserId)
                .ToListAsync();

            lists.Should().HaveCount(2);
            lists.Select(l => l.Name).Should().Contain(new[] { "Owned", "Wanted" });
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenUserAlreadyExists()
        {
            using var context = DbContextFactory.Create();

            context.Users.Add(new User
            {
                Username = "user",
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
    }
}
