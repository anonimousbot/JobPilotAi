namespace JobPilotAi_Backend.Modules.Billing;

public sealed class UsageRecord
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public UsageActionType ActionType { get; private set; }

    public int Quantity { get; private set; }

    public int? TokensUsed { get; private set; }

    public int? ProcessingTimeMs { get; private set; }

    public decimal? EstimatedCost { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }

    public DateTimeOffset PeriodEnd { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static UsageRecord Create(
        Guid userId,
        UsageActionType actionType,
        int quantity,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        DateTimeOffset createdAt,
        int? tokensUsed = null,
        int? processingTimeMs = null,
        decimal? estimatedCost = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionType = actionType,
            Quantity = quantity,
            TokensUsed = tokensUsed,
            ProcessingTimeMs = processingTimeMs,
            EstimatedCost = estimatedCost,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            CreatedAt = createdAt
        };
}
