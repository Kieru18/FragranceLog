using System.Text.Json;
using Core.Enums;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Enums
{
    public sealed class KebabCaseEnumConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public KebabCaseEnumConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new KebabCaseEnumConverter<TestEnum>());
        }

        [Fact]
        public void Write_ShouldSerializeSingleWordEnum_ToLowercase()
        {
            var json = JsonSerializer.Serialize(TestEnum.Simple, _options);

            json.Should().Be("\"simple\"");
        }

        [Fact]
        public void Write_ShouldSerializeMultiWordEnum_ToKebabCase()
        {
            var json = JsonSerializer.Serialize(TestEnum.MultiWordValue, _options);

            json.Should().Be("\"multi-word-value\"");
        }

        [Fact]
        public void Read_ShouldDeserializeSingleWordEnum_FromLowercase()
        {
            var result = JsonSerializer.Deserialize<TestEnum>("\"simple\"", _options);

            result.Should().Be(TestEnum.Simple);
        }

        [Fact]
        public void Read_ShouldDeserializeMultiWordEnum_FromKebabCase()
        {
            var result = JsonSerializer.Deserialize<TestEnum>("\"multi-word-value\"", _options);

            result.Should().Be(TestEnum.MultiWordValue);
        }

        [Fact]
        public void Read_ShouldThrowJsonException_WhenValueIsNull()
        {
            Action act = () =>
                JsonSerializer.Deserialize<TestEnum>("null", _options);

            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void Read_ShouldThrowArgumentException_WhenEnumValueDoesNotExist()
        {
            Action act = () =>
                JsonSerializer.Deserialize<TestEnum>("\"does-not-exist\"", _options);

            act.Should().Throw<ArgumentException>();
        }

        private enum TestEnum
        {
            Simple,
            MultiWordValue
        }
    }
}
