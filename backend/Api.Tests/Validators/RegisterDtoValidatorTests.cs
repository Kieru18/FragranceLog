using Api.Validators;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Validators
{
    public sealed class RegisterDtoValidatorTests
    {
        private readonly RegisterDtoValidator _validator;

        public RegisterDtoValidatorTests()
        {
            _validator = new RegisterDtoValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenUsernameIsNullOrEmpty(string value)
        {
            var dto = CreateValidDto();
            dto.Username = value;

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Username));
        }

        [Theory]
        [InlineData("ab")]
        public void ShouldHaveError_WhenUsernameIsTooShort(string value)
        {
            var dto = CreateValidDto();
            dto.Username = value;

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Username));
        }

        [Fact]
        public void ShouldHaveError_WhenUsernameIsTooLong()
        {
            var dto = CreateValidDto();
            dto.Username = new string('a', 51);

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Username));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(50)]
        public void ShouldBeValid_WhenUsernameLengthIsAtBoundary(int length)
        {
            var dto = CreateValidDto();
            dto.Username = new string('a', length);

            var result = _validator.Validate(dto);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.Username));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenEmailIsNullOrEmpty(string value)
        {
            var dto = CreateValidDto();
            dto.Email = value;

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsInvalid()
        {
            var dto = CreateValidDto();
            dto.Email = "not-an-email";

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Fact]
        public void ShouldHaveError_WhenEmailIsTooLong()
        {
            var dto = CreateValidDto();
            dto.Email = new string('a', 101) + "@test.com";

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        }

        [Theory]
        [InlineData("short1!")]
        [InlineData("alllowercase1!")]
        [InlineData("ALLUPPERCASE1!")]
        [InlineData("NoDigits!")]
        [InlineData("NoSpecial1")]
        public void ShouldHaveError_WhenPasswordDoesNotMeetComplexityRules(string password)
        {
            var dto = CreateValidDto();
            dto.Password = password;

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(dto.Password) &&
                e.ErrorMessage == "Password must contain uppercase, lowercase, digit, special character and be at least 8 characters long."
            );
        }

        [Fact]
        public void ShouldBeValid_WhenPasswordMeetsAllRequirements()
        {
            var dto = CreateValidDto();
            dto.Password = "ValidPassword1!";

            var result = _validator.Validate(dto);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.Password));
        }

        [Fact]
        public void ShouldBeValid_WhenAllFieldsAreValid()
        {
            var dto = CreateValidDto();

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        private static RegisterDto CreateValidDto()
        {
            return new RegisterDto
            {
                Username = "validuser",
                Email = "user@example.com",
                Password = "ValidPassword1!"
            };
        }
    }
}
