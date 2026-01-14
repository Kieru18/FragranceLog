using Api.Validators;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Validators
{
    public sealed class ChangePasswordDtoValidatorTests
    {
        private readonly ChangePasswordDtoValidator _validator;

        public ChangePasswordDtoValidatorTests()
        {
            _validator = new ChangePasswordDtoValidator();
        }

        [Fact]
        public void ShouldHaveError_WhenCurrentPasswordIsEmpty()
        {
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "",
                NewPassword = "Valid1!"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(dto.CurrentPassword));
        }

        [Fact]
        public void ShouldHaveError_WhenNewPasswordIsEmpty()
        {
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword1!",
                NewPassword = ""
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.NewPassword));
        }



        [Theory]
        [InlineData("short1!")]
        [InlineData("alllowercase1!")]
        [InlineData("ALLUPPERCASE1!")]
        [InlineData("NoDigits!")]
        [InlineData("NoSpecial1")]
        public void ShouldHaveError_WhenNewPasswordDoesNotMeetComplexityRules(string password)
        {
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword1!",
                NewPassword = password
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == nameof(dto.NewPassword) &&
                e.ErrorMessage == "Password must contain uppercase, lowercase, digit, special character and be at least 8 characters long."
            );
        }

        [Fact]
        public void ShouldBeValid_WhenPasswordsMeetAllRequirements()
        {
            var dto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword1!",
                NewPassword = "NewPassword1!"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
