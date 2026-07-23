using FluentValidation;

namespace JobPilotAi_Backend.Features.CoverLetters;

public sealed class CreateCoverLetterRequestValidator : AbstractValidator<CreateCoverLetterRequest>
{
    private static readonly string[] SupportedTones = ["Professional", "Friendly", "Confident", "Executive"];

    public CreateCoverLetterRequestValidator()
    {
        RuleFor(request => request.JobTitle).NotEmpty().MaximumLength(160);
        RuleFor(request => request.CompanyName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.JobDescription).NotEmpty().MaximumLength(12000);
        RuleFor(request => request.Tone)
            .Must(tone => string.IsNullOrWhiteSpace(tone) || SupportedTones.Contains(tone, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Tone must be Professional, Friendly, Confident, or Executive.");
    }
}

public sealed class UpdateCoverLetterRequestValidator : AbstractValidator<UpdateCoverLetterRequest>
{
    public UpdateCoverLetterRequestValidator()
    {
        RuleFor(request => request.EditedContent).NotEmpty().MaximumLength(20000);
    }
}
