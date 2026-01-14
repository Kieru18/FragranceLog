using System.Security.Claims;
using Core.Extensions;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Extensions
{
    public sealed class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetUserId_ShouldReturnNull_WhenUserIsNull()
        {
            ClaimsPrincipal? user = null;

            var result = user.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenIdentityIsNull()
        {
            var user = new ClaimsPrincipal();

            var result = user.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenUserIsNotAuthenticated()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            var result = user.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenNameIdentifierClaimIsMissing()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Email, "test@test.com") },
                authenticationType: "Test"
            );

            var user = new ClaimsPrincipal(identity);

            var result = user.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenNameIdentifierIsNotAnInteger()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "abc") },
                authenticationType: "Test"
            );

            var user = new ClaimsPrincipal(identity);

            var result = user.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturnUserId_WhenNameIdentifierIsValidInteger()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "42") },
                authenticationType: "Test"
            );

            var user = new ClaimsPrincipal(identity);

            var result = user.GetUserId();

            result.Should().Be(42);
        }
    }
}
