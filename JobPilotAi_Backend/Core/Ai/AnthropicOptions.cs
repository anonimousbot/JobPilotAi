namespace JobPilotAi_Backend.Core.Ai;

public sealed class AnthropicOptions
{
    public const string SectionName = "Ai:Anthropic";

    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = "https://api.anthropic.com";

    public string Model { get; init; } = "claude-sonnet-4-5";
}
