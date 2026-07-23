using FluentValidation;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;

namespace JobPilotAi_Backend.Features.System.Health;

public static class HealthFeature
{
    public static IEndpointRouteBuilder MapHealthFeature(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/health")
            .WithTags("Health");

        group
            .MapGet("", GetLiveness)
            .WithName("GetLiveness")
            .WithSummary("Returns a lightweight liveness signal.");

        group
            .MapGet("/ready", GetReadiness)
            .WithName("GetReadiness")
            .WithSummary("Returns readiness, including required dependency checks.");

        return app;
    }

    private static IResult GetLiveness() =>
        TypedResults.Ok(new LivenessResponse("Healthy", DateTimeOffset.UtcNow));

    private static async Task<IResult> GetReadiness(
        [AsParameters] GetReadinessRequest request,
        IValidator<GetReadinessRequest> validator,
        ApplicationDbContext dbContext,
        DatabaseConnectionOptions databaseOptions,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<ReadinessResponse>
                .Failure(Error.Validation(validationResult.ToErrorDictionary()))
                .ToHttpResult();
        }

        var result = await GetReadinessHandler.HandleAsync(
            request,
            dbContext,
            databaseOptions,
            cancellationToken);

        return result.ToHttpResult();
    }
}
