using FluentValidation;

namespace JobPilotAi_Backend.Features.System.Health;

internal sealed class GetReadinessRequestValidator : AbstractValidator<GetReadinessRequest>
{
    public GetReadinessRequestValidator()
    {
        RuleFor(request => request.TimeoutSeconds)
            .InclusiveBetween(1, 10)
            .WithMessage("TimeoutSeconds must be between 1 and 10.");
    }
}
