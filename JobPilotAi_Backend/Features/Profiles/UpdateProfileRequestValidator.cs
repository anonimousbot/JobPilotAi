using FluentValidation;

namespace JobPilotAi_Backend.Features.Profiles;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.PhoneNumber).MaximumLength(40);
        RuleFor(request => request.Location).MaximumLength(160);
        RuleFor(request => request.LinkedInUrl).MaximumLength(500).Must(BeValidUrl).When(request => !string.IsNullOrWhiteSpace(request.LinkedInUrl));
        RuleFor(request => request.PortfolioUrl).MaximumLength(500).Must(BeValidUrl).When(request => !string.IsNullOrWhiteSpace(request.PortfolioUrl));
        RuleFor(request => request.CareerGoal).MaximumLength(80);
        RuleFor(request => request.TargetRole).MaximumLength(160);
        RuleFor(request => request.CareerLevel).MaximumLength(80);
    }

    private static bool BeValidUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public sealed class UpdateOnboardingProfileRequestValidator : AbstractValidator<UpdateOnboardingProfileRequest>
{
    private static readonly string[] SupportedCareerGoals =
    [
        "land-new-job",
        "switch-careers",
        "earn-promotion",
        "just-exploring"
    ];

    public UpdateOnboardingProfileRequestValidator()
    {
        RuleFor(request => request.CareerGoal)
            .NotEmpty()
            .MaximumLength(80)
            .Must(goal => SupportedCareerGoals.Contains(goal, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Career goal must be land-new-job, switch-careers, earn-promotion, or just-exploring.");

        RuleFor(request => request.TargetRole).NotEmpty().MaximumLength(160);
        RuleFor(request => request.CareerLevel).NotEmpty().MaximumLength(80);
        RuleFor(request => request.CurrentJobTitle).MaximumLength(160);
        RuleFor(request => request.YearsOfExperience).GreaterThanOrEqualTo(0).LessThanOrEqualTo(50);
    }
}
