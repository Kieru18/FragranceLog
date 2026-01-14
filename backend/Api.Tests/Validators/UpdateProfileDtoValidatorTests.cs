using Api.Validators;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Validators
{
    public sealed class UpdateProfileDtoValidatorTests
    {
        private readonly UpdateProfileDtoValidator _validator;

        public UpdateProfileDtoValidatorTests()
        {
            _validator = new UpdateProfileDtoValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenDisplayNameIsNullOrEmpty(string value)
        {
            var dto = CreateDto(value, "user@example.com");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
        }

        [Theory]
        [InlineData("ab")]
        public void ShouldHaveError_WhenDisplayNameIsTooShort(string value)
        {
            var dto = CreateDto(value, "user@example.com");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
        }

        [Fact]
        public void ShouldHaveError_WhenDisplayNameIsTooLong()
        {
            var dto = CreateDto(new string('a', 51), "user@example.com");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(50)]
        public void ShouldBeValid_WhenDisplayNameLengthIsAtBoundary(int length)
        {
            var dto = CreateDto(new string('a', length), "user@example.com");

            var result = _validator.Validate(dto);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.DisplayName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenEmailIsNullOrEmpty(string value)
        {
            var dto = CreateDto("Valid User", value);

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsInvalid()
        {
            var dto = CreateDto("Valid User", "not-an-email");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsTooLong()
        {
            var dto = CreateDto("Valid User", new string('a', 101) + "@test.com");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Fact]
        public void ShouldBeValid_WhenAllFieldsAreValid()
        {
            var dto = CreateDto("Valid User", "user@example.com");

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        private static UpdateProfileDto CreateDto(string displayName, string email)
        {
            return new UpdateProfileDto
            {
                DisplayName = displayName,
                Email = email
            };
        }
    }
}
