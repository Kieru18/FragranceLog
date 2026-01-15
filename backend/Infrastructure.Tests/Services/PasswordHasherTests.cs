using FluentAssertions;
using Infrastructure.Services;
using Xunit;

namespace Infrastructure.Tests.Services
{
    public sealed class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void Hash_ShouldProduceNonEmptyHash()
        {
            var hash = _hasher.Hash("Password1!");

            hash.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Hash_ShouldProduceDifferentHashes_ForSamePassword()
        {
            var hash1 = _hasher.Hash("Password1!");
            var hash2 = _hasher.Hash("Password1!");

            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void Hash_ShouldUseExpectedFormat()
        {
            var hash = _hasher.Hash("Password1!");

            var parts = hash.Split('.', 3);

            parts.Should().HaveCount(3);
            parts[0].Should().Be("100000");
            parts[1].Should().NotBeNullOrWhiteSpace();
            parts[2].Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Verify_ShouldReturnTrue_ForCorrectPassword()
        {
            var password = "Password1!";
            var hash = _hasher.Hash(password);

            var result = _hasher.Verify(password, hash);

            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForWrongPassword()
        {
            var hash = _hasher.Hash("Password1!");

            var result = _hasher.Verify("WrongPassword!", hash);

            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForMalformedHash()
        {
            var result = _hasher.Verify("Password1!", "invalid-hash");

            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenIterationCountIsInvalid()
        {
            var hash = "notanumber.salt.key";

            var result = _hasher.Verify("Password1!", hash);

            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenSaltIsTampered()
        {
            var password = "Password1!";
            var hash = _hasher.Hash(password);

            var parts = hash.Split('.', 3);
            parts[1] = Convert.ToBase64String(new byte[16]);

            var tampered = string.Join('.', parts);

            var result = _hasher.Verify(password, tampered);

            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenKeyIsTampered()
        {
            var password = "Password1!";
            var hash = _hasher.Hash(password);

            var parts = hash.Split('.', 3);
            parts[2] = Convert.ToBase64String(new byte[32]);

            var tampered = string.Join('.', parts);

            var result = _hasher.Verify(password, tampered);

            result.Should().BeFalse();
        }
    }
}
