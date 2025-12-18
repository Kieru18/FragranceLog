using Core.DTOs;
using FluentValidation;

namespace Api.Validators
{
    public sealed class CreateReviewDtoValidator : AbstractValidator<SaveReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.PerfumeId)
                .GreaterThan(0);

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5);

            RuleFor(x => x.Text)
                .MaximumLength(2000);
        }
    }
}
