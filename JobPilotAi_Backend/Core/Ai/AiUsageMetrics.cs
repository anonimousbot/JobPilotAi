namespace JobPilotAi_Backend.Core.Ai;

public sealed record AiUsageMetrics(int TokensUsed, int ProcessingTimeMs, decimal EstimatedCost);
