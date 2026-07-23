namespace JobPilotAi_Backend.Modules.Admin;

public sealed class AdminLog
{
    public Guid Id { get; private set; }

    public Guid AdminId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string TargetEntity { get; private set; } = string.Empty;

    public Guid? TargetId { get; private set; }

    public string MetadataJson { get; private set; } = "{}";

    public DateTimeOffset CreatedAt { get; private set; }
}
