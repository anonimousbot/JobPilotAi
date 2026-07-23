using FluentValidation;

namespace JobPilotAi_Backend.Features.Analyses;

public sealed class CreateAnalysisRequestValidator : AbstractValidator<CreateAnalysisRequest>
{
    public CreateAnalysisRequestValidator()
    {
        RuleFor(request => request.ResumeId)
            .NotEmpty()
            .Must(resumeId => Guid.TryParse(resumeId, out _))
            .WithMessage("ResumeId must be a valid resume id.");

        RuleFor(request => request.JobDescription).MaximumLength(12000);
    }
}
