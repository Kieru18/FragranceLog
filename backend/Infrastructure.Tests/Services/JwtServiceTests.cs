using Core.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Infrastructure.Tests.Services
{
    public sealed class JwtServiceTests
    {
        private static JwtService CreateService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "THIS_IS_A_TEST_KEY_1234567890123456",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience"
                })
                .Build();

            return new JwtService(config);
        }

        [Fact]
        public void GenerateAccessToken_ShouldProduceValidToken_WithExpectedClaims()
        {
            var service = CreateService();

            var user = new User
            {
                UserId = 42,
                Username = "testuser",
                Email = "test@test.com"
            };

            var token = service.GenerateAccessToken(user);

            token.Should().NotBeNullOrWhiteSpace();

            var principal = service.ValidateAccessToken(token);

            principal.Should().NotBeNull();

            principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value
                .Should().Be("42");

            principal.FindFirst(ClaimTypes.Name)!.Value
                .Should().Be("testuser");

            principal.FindFirst(ClaimTypes.Email)!.Value
                .Should().Be("test@test.com");
        }

        [Fact]
        public void ValidateAccessToken_ShouldReturnNull_ForInvalidToken()
        {
            var service = CreateService();

            var principal = service.ValidateAccessToken("invalid.token.value");

            principal.Should().BeNull();
        }

        [Fact]
        public void GenerateRefreshToken_ShouldProduceValidToken_WithUserIdClaim()
        {
            var service = CreateService();

            var token = service.GenerateRefreshToken(99);

            token.Should().NotBeNullOrWhiteSpace();

            var principal = service.ValidateRefreshToken(token);

            principal.Should().NotBeNull();

            principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value
                .Should().Be("99");
        }

        [Fact]
        public void ValidateRefreshToken_ShouldReturnNull_ForInvalidToken()
        {
            var service = CreateService();

            var principal = service.ValidateRefreshToken("invalid.token.value");

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateAccessToken_ShouldFail_WhenAudienceIsWrong()
        {
            var validService = CreateService();

            var user = new User
            {
                UserId = 1,
                Username = "user",
                Email = "user@test.com"
            };

            var token = validService.GenerateAccessToken(user);

            var wrongConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "THIS_IS_A_TEST_KEY_1234567890123456",
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "wrong-audience"
                })
                .Build();

            var wrongService = new JwtService(wrongConfig);

            var principal = wrongService.ValidateAccessToken(token);

            principal.Should().BeNull();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenJwtKeyIsMissing()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "issuer",
                    ["Jwt:Audience"] = "audience"
                })
                .Build();

            var service = new JwtService(config);

            FluentActions
                .Invoking(() => service.GenerateRefreshToken(1))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("JWT Key not present in configuration.");
        }
    }
}
