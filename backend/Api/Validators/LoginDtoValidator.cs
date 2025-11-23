using Core.DTOs;
using FluentValidation;

namespace Api.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty()
                .WithMessage("Username or email is required.");

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
