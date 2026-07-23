namespace JobPilotAi_Backend.Core.Ai;

public sealed class OpenAiOptions
{
    public const string SectionName = "Ai:OpenAI";

    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = "https://api.openai.com/v1";

    public string Model { get; init; } = "gpt-5.4-mini";

    public int TimeoutSeconds { get; init; } = 45;

    public decimal EstimatedInputCostPerMillionTokens { get; init; } = 0.75m;

    public decimal EstimatedOutputCostPerMillionTokens { get; init; } = 4.50m;
}
