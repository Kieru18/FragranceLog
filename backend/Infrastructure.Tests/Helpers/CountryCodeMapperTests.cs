using FluentAssertions;
using Infrastructure.Helpers;
using Xunit;

namespace Infrastructure.Tests.Helpers
{
    public sealed class CountryCodeMapperTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ToAlpha3_ShouldReturnNull_WhenInputIsNullOrWhitespace(string input)
        {
            var result = CountryCodeMapper.ToAlpha3(input);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData("PL", "POL")]
        [InlineData("US", "USA")]
        [InlineData("DE", "DEU")]
        [InlineData("GB", "GBR")]
        public void ToAlpha3_ShouldReturnAlpha3Code_ForKnownAlpha2Code(string alpha2, string expected)
        {
            var result = CountryCodeMapper.ToAlpha3(alpha2);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("pl", "POL")]
        [InlineData("uS", "USA")]
        [InlineData("gB", "GBR")]
        public void ToAlpha3_ShouldBeCaseInsensitive(string alpha2, string expected)
        {
            var result = CountryCodeMapper.ToAlpha3(alpha2);

            result.Should().Be(expected);
        }

        [Fact]
        public void ToAlpha3_ShouldReturnNull_WhenAlpha2CodeIsUnknown()
        {
            var result = CountryCodeMapper.ToAlpha3("XX");

            result.Should().BeNull();
        }
    }
}
