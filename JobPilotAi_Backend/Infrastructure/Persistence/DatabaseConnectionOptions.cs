namespace JobPilotAi_Backend.Infrastructure.Persistence;

public sealed record DatabaseConnectionOptions(string? ConnectionString)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
}
