namespace JobPilotAi_Backend.Core.Ai;

public sealed class GeminiOptions
{
    public const string SectionName = "Ai:Gemini";

    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta/openai/";

    public string Model { get; init; } = "gemini-1.5-flash";

    public string FallbackApiKey { get; init; } = string.Empty;

    public string FallbackBaseUrl { get; init; } = "https://integrate.api.nvidia.com/v1";

    public string FallbackModel { get; init; } = "google/gemma-2-27b-it";

    public int TimeoutSeconds { get; init; } = 120;

    public decimal EstimatedInputCostPerMillionTokens { get; init; } = 0.075m;

    public decimal EstimatedOutputCostPerMillionTokens { get; init; } = 0.30m;
}
