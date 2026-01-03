using Core.DTOs;
using FluentValidation;

namespace Api.Validators;

public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,}$")
            .WithMessage(
                "Password must contain uppercase, lowercase, digit, special character and be at least 8 characters long."
            );
    }
}
