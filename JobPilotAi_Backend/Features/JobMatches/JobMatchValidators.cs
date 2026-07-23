using FluentValidation;

namespace JobPilotAi_Backend.Features.JobMatches;

public sealed class JobMatchSearchRequestValidator : AbstractValidator<JobMatchSearchRequest>
{
    public JobMatchSearchRequestValidator()
    {
        RuleFor(request => request.Query).MaximumLength(160);
        RuleFor(request => request.Location).MaximumLength(160);
        RuleFor(request => request.ResumeId).Must(BeGuid).When(request => !string.IsNullOrWhiteSpace(request.ResumeId));
    }

    private static bool BeGuid(string? value) =>
        Guid.TryParse(value, out _);
}
