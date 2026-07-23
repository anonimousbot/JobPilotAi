namespace JobPilotAi_Backend.Core.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = "development-only-change-this-jobpilotai-jwt-key";

    public string Issuer { get; init; } = "JobPilotAi";

    public int AccessTokenMinutes { get; init; } = 15;
}
