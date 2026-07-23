namespace JobPilotAi_Backend.Features.System.Health;

public sealed record LivenessResponse(
    string Status,
    DateTimeOffset CheckedAt);

public sealed record ReadinessResponse(
    string Status,
    DateTimeOffset CheckedAt,
    IReadOnlyDictionary<string, DependencyHealth>? Dependencies);

public sealed record DependencyHealth(string Status);
