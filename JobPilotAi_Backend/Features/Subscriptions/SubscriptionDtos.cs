namespace JobPilotAi_Backend.Features.Subscriptions;

public sealed record SubscriptionResponse(
    Guid Id,
    string PlanType,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    string BillingCycle);
