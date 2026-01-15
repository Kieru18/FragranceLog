using Api.Validators;
using Core.DTOs;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Validators
{
    public sealed class CreateReviewDtoValidatorTests
    {
        private readonly CreateReviewDtoValidator _validator;

        public CreateReviewDtoValidatorTests()
        {
            _validator = new CreateReviewDtoValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldHaveError_WhenPerfumeIdIsNotGreaterThanZero(int perfumeId)
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = perfumeId,
                Rating = 3,
                Text = "ok"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.PerfumeId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void ShouldHaveError_WhenRatingIsOutsideAllowedRange(int rating)
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = rating,
                Text = "ok"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Rating));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ShouldBeValid_WhenRatingIsAtBoundary(int rating)
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = rating,
                Text = "ok"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeValid_WhenTextIsNull()
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = 3,
                Text = null
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeValid_WhenTextIsEmpty()
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = 3,
                Text = ""
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeValid_WhenTextLengthIsExactly2000()
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = 3,
                Text = new string('a', 2000)
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ShouldHaveError_WhenTextLengthExceeds2000()
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = 3,
                Text = new string('a', 2001)
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Text));
        }

        [Fact]
        public void ShouldBeValid_WhenAllFieldsAreValid()
        {
            var dto = new SaveReviewDto
            {
                PerfumeId = 1,
                Rating = 5,
                Text = "Great scent"
            };

            var result = _validator.Validate(dto);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}
