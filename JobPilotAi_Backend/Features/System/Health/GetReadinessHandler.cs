using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Infrastructure.Persistence;

namespace JobPilotAi_Backend.Features.System.Health;

internal static class GetReadinessHandler
{
    public static async Task<Result<ReadinessResponse>> HandleAsync(
        GetReadinessRequest request,
        ApplicationDbContext dbContext,
        DatabaseConnectionOptions databaseOptions,
        CancellationToken cancellationToken)
    {
        if (!databaseOptions.IsConfigured)
        {
            return Result<ReadinessResponse>.Failure(
                Error.Unavailable(
                    "database.connection_string_missing",
                    "ConnectionStrings:Default must be configured before the API is ready."));
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(request.TimeoutSeconds));

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(timeout.Token);
            if (!canConnect)
            {
                return Result<ReadinessResponse>.Failure(
                    Error.Unavailable(
                        "database.unavailable",
                        "The database could not be reached."));
            }

            return Result<ReadinessResponse>.Success(
                new ReadinessResponse(
                    "Ready",
                    DateTimeOffset.UtcNow,
                    request.IncludeDetails ? CreateDependencyDetails("database", "Healthy") : null));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<ReadinessResponse>.Failure(
                Error.Unavailable(
                    "database.timeout",
                    $"The database did not respond within {request.TimeoutSeconds} seconds."));
        }
        catch (Exception exception)
        {
            return Result<ReadinessResponse>.Failure(
                Error.Unexpected(
                    "database.health_check_failed",
                    exception.Message));
        }
    }

    private static IReadOnlyDictionary<string, DependencyHealth> CreateDependencyDetails(
        string dependency,
        string status) =>
        new Dictionary<string, DependencyHealth>(StringComparer.OrdinalIgnoreCase)
        {
            [dependency] = new(status)
        };
}
