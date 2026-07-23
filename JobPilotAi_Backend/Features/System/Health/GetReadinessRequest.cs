namespace JobPilotAi_Backend.Features.System.Health;

public sealed record GetReadinessRequest(
    bool IncludeDetails = false,
    int TimeoutSeconds = 2);
