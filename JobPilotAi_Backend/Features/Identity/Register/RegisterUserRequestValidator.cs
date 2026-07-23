using FluentValidation;

namespace JobPilotAi_Backend.Features.Identity.Register;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}
