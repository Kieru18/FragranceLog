using Api.Validators;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Validators
{
    public sealed class LoginDtoValidatorTests
    {
        private readonly LoginDtoValidator _validator;

        public LoginDtoValidatorTests()
        {
            _validator = new LoginDtoValidator();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenUsernameOrEmailIsNullOrEmpty(string value)
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = value,
                Password = "password"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(dto.UsernameOrEmail) &&
                e.ErrorMessage == "Username or email is required."
            );
        }

        [Fact]
        public void ShouldBeValid_WhenUsernameOrEmailIsProvided()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "user@example.com",
                Password = "password"
            };

            var result = _validator.Validate(dto);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.UsernameOrEmail));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShouldHaveError_WhenPasswordIsNullOrEmpty(string value)
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "user",
                Password = value
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(dto.Password)
            );
        }

        [Fact]
        public void ShouldBeValid_WhenPasswordIsProvided()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "user",
                Password = "password"
            };

            var result = _validator.Validate(dto);

            result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.Password));
        }

        [Fact]
        public void ShouldBeValid_WhenAllFieldsAreValid()
        {
            var dto = new LoginDto
            {
                UsernameOrEmail = "user@example.com",
                Password = "password"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
